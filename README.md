# Program Designer API

A .NET (C#) REST API that lets non-technical staff define the structure of any
learning program — steps, nested groups, ordering rules, and prerequisites —
and validates that structure for logical errors and reachability risks.


---

## Tech Stack

- **.NET 10 / C#**
- **Entity Framework Core** (Code First, migrations, SQL Server / LocalDB )
- **AutoMapper** — entity → DTO mapping
- **FluentValidation** — request shape validation
- **Scalar** — interactive OpenAPI documentation (replaces Swagger UI)
- **xUnit** — unit tests

## Architecture

The solution follows a **3-layer architecture** plus a shared kernel project:

```
ProgramDesigner.Common   →  Enums, GeneralResult<T> (Result Pattern), Error
ProgramDesigner.DAL      →  Entities, EF Core DbContext, Repositories, UnitOfWork
ProgramDesigner.BLL      →  DTOs, AutoMapper Profiles, FluentValidation Validators,
                             the Program Manager, and the Validation Engine
ProgramDesigner.API      →  Controllers, Program.cs wiring, Scalar docs
ProgramDesigner.Tests    →  xUnit tests for the validation and simulation engines
```

Dependency direction is one-way: `API → BLL → DAL → Common`. Each layer only
knows about the one below it, registering its own services through a
`ServicesExtension` so `Program.cs` stays a thin composition root.

Key patterns used throughout: **Repository Pattern**, **Unit of Work**,
**General Result Pattern** (`GeneralResult<T>` — every business operation
returns `Success`, `Data`, and a list of `Error`s instead of throwing for
expected failure cases), **AutoMapper**, and **DTOs** at every API boundary.

---

## Data Model

### Why Step and Group share one table (TPH)

A `Group` must be able to hold a mixed, ordered list of `Step`s and other
`Group`s, nested to any depth. To represent that faithfully, `Step` and
`Group` both inherit from an abstract `ProgramItem`, mapped with EF Core's
**Table-Per-Hierarchy (TPH)** strategy — one physical table (`ProgramItems`)
with a discriminator column (`ItemType`). This is what makes a `Group.Children`
collection able to hold both types together, in one true order, at any depth,
without a fixed schema per level.

```
ProgramItems
├── Id, Name, Order, ParentGroupId (FK, self), PrerequisiteItemId (FK, self)
├── ItemType (discriminator: "Step" | "Group")
├── StepType        (Step only)
└── RuleType, ChoiceCount   (Group only)
```

- **Prerequisite** is a single nullable self-referencing FK
  (`PrerequisiteItemId`) directly on `ProgramItem` — any item can point at
  any other item, matching the challenge's "any step or group can have a
  prerequisite" requirement without an extra join table.
- Both self-referencing FKs (`ParentGroupId`, `PrerequisiteItemId`) use
  `ON DELETE NO ACTION`. SQL Server rejects `CASCADE` on multiple
  self-references into the same table (multiple cascade paths), and it also
  gives us the chance to control deletion logic explicitly at the BLL layer
  later if needed.
- `LearningProgram` is a thin wrapper (`Id`, `Name`, `CreatedAt`,
  `RootGroupId`) around the tree — it gives a program its own identity
  separate from "being a Group."

### Why the create request uses temporary `Key`/`PrerequisiteKey`

When a program is created, none of its items have real database IDs yet.
So the request format lets the caller assign a temporary, request-scoped
`Key` to every node, and reference another node's `Key` via
`PrerequisiteKey`. The API resolves these into real entity references
in memory, and EF Core assigns the actual IDs once the whole tree is saved
in one transaction.

---

## Setup Instructions

**Prerequisites:** .NET SDK 10+ and SQL Server (LocalDB, Express, or full) 

1. Clone the repository.
2. Update the connection string in `ProgramDesigner.API/appsettings.json`
   if needed (defaults to LocalDB):
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ProgramDesignerDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
3. From the solution root:
   ```bash
   dotnet ef database update --project ProgramDesigner.DAL --startup-project ProgramDesigner.API
   dotnet run --project ProgramDesigner.API
   ```
4. On first run, the database is automatically seeded with the full
   **Computer Science** scenario from the challenge brief.
5. Run the application. Scalar API Reference will open automatically in your default browser. If it doesn't, navigate to:
https://localhost:{port}/scalar/v1

Use Scalar to explore and test all available endpoints.

Total time from clone to a running, seeded API: under 5 minutes.

---

## API Reference

### `POST /programs`

Creates a program from a JSON tree. `Key` is a temporary, request-only
identifier used to wire up prerequisites within the same request;
`PrerequisiteKey` references another node's `Key` anywhere in the tree.

```json
{
  "name": "Computer Science",
  "rootGroup": {
    "key": "root",
    "name": "Computer Science",
    "itemType": "Group",
    "ruleType": "InOrder",
    "children": [
      {
        "key": "foundations",
        "name": "Foundations",
        "itemType": "Group",
        "ruleType": "InOrder",
        "children": [
          { "key": "s1", "name": "Introduction to Computing", "itemType": "Step", "stepType": "Attend Session" }
        ]
      },
      {
        "key": "major",
        "name": "Major",
        "itemType": "Group",
        "ruleType": "Choice",
        "choiceCount": 1,
        "prerequisiteKey": "foundations",
        "children": []
      }
    ]
  }
}
```

Returns `201 Created` with the saved program (real IDs assigned), or `400`
with a list of errors if the shape is invalid or a `PrerequisiteKey` doesn't
resolve to anything in the request.

### `GET /programs/{id}`

Returns the full nested structure of a program, with real IDs throughout.
Returns `404` if the program doesn't exist.

### `POST /programs/{id}/validate`

Runs validation and returns:

```json
{
  "isValid": true,
  "impossiblePrerequisites": [],
  "reachabilityWarnings": []
}
```

`isValid` is `false` only when `impossiblePrerequisites` is non-empty.
Reachability warnings never block validity.

### `POST /programs/{id}/simulate` (bonus)

Stateless — nothing here is persisted. Given a participant's completed steps
and the branches they picked in choice groups, returns the current state of
every item in the tree.

```json
{
  "choices": { "3": [6] },
  "completedItemIds": [4, 5]
}
```

`choices` maps a Choice group's `Id` to the `Id`s of the children the
participant selected. `completedItemIds` lists the `Id`s of Steps already
done. Both are optional — an empty body simulates a participant who hasn't
started or decided anything yet.

```json
{
  "items": [
    { "itemId": 6, "itemName": "AI", "itemType": "Group", "status": "Unlocked", "reason": null },
    { "itemId": 7, "itemName": "IT", "itemType": "Group", "status": "Excluded", "reason": "This branch was not selected in the participant's choices, so it can never be completed." },
    { "itemId": 9, "itemName": "Final Capstone", "itemType": "Step", "status": "Blocked", "reason": "Waiting on prerequisite 'Major' to be completed." }
  ]
}
```

Every item gets one of four statuses:
- **Complete** — a Step in `completedItemIds`, or a Group whose rule is
  already satisfied (all children for `InOrder`, enough completed children
  for `Choice`).
- **Unlocked** — can be attempted right now.
- **Blocked** — waiting on a prerequisite, an earlier sibling in an `InOrder`
  group, or a parent group that isn't unlocked yet (status is inherited down
  the tree).
- **Excluded** — sits inside a choice branch the participant didn't pick;
  can never be completed.

---

## Validation Logic

Validation runs on an in-memory tree rebuilt from the database (all
`ProgramItem` rows are loaded once and reassembled via `ParentGroupId` —
practical at the scale of learning programs, and it avoids the arbitrary-depth
`Include()` chains EF Core can't express). Each node is given an **enter/exit
order** via a single depth-first pass (an Euler-tour-style interval), which
turns "is X inside Y?" and "does X appear after Y?" into O(1) integer
comparisons instead of repeated tree walks.

**Impossible prerequisites** (rejected — `IsValid: false`) are detected per
prerequisite edge, in order:
1. The item points at itself.
2. The item points at something nested inside itself.
3. The item points at something that appears later in the program.

A direct A↔B cycle is caught automatically by rule 3, since one of the two
items necessarily comes later in the enter-order regardless of which one you
check first — no separate graph-cycle detection was needed.

**Reachability warnings** (non-blocking) are found by walking up from the
*prerequisite target* through its ancestors. If any ancestor is a `Choice`
group where `ChoiceCount < children.Count`, the target sits inside a branch
that isn't guaranteed to be picked. The one deliberate refinement here: if the
*dependent item itself* lives inside that same branch, there's no real risk —
it can only ever be attempted by a participant who already took that branch,
so the warning is suppressed. This is what correctly keeps
`Final Capstone → Major` warning-free (the target is the choice group itself)
while still flagging a hypothetical `Final Capstone → AI Capstone` prerequisite
(the target sits one branch deep, unrelated to the dependent item's own path).

---

## Testing

```bash
dotnet test
```

`ProgramValidationEngineTests` covers the four required scenarios directly
against the validation engine (no database involved — trees are built by
hand in-memory), matching the challenge's Part 3 requirements:

- The full Computer Science scenario validates with no errors or warnings.
- A direct prerequisite cycle is rejected.
- A prerequisite pointing at itself is rejected.
- A prerequisite inside an unguaranteed choice branch produces a warning,
  not a rejection.

`ProgramSimulationEngineTests` covers the bonus `/simulate` logic the same
way — a fresh start where only the first step is unlocked, an undecided
choice group where no branch is excluded yet, a decided choice where the
other branches become excluded, and a fully completed track that unlocks
what comes after it.

---

## AI Tool Usage

During development, I occasionally used AI as a technical assistant for 
discussing design ideas,exploring alternative solutions, and troubleshooting 
specific issues. However, the project architecture, technical decisions, validation 
logic, debugging, and testing were designed, written, and verified by me.
