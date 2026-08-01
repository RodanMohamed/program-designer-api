# Program Designer API

A .NET (C#) REST API that lets non-technical staff define the structure of any
learning program — steps, nested groups, ordering rules, and one or more
prerequisites per item — and validates that structure for logical errors and
reachability risks.

A separate Angular frontend that consumes this API is available at:
**https://github.com/RodanMohamed/program-designer-ui.git** — see that repo's
own README for setup instructions. The API works completely standalone (via
Scalar) without it.

---

## Tech Stack

- **.NET 10 / C#**
- **Entity Framework Core** (Code First, migrations, SQL Server / LocalDB)
- **Scalar** — interactive OpenAPI documentation (replaces Swagger UI)
- **xUnit** — unit tests


## Architecture

This is a **Domain-Driven Design** solution, chosen deliberately over a
simpler layered/N-Tier approach because the domain itself has real business
rules worth protecting: a step or group can never depend on itself or on
something nested inside it, a "pick N of M" choice must stay consistent with
however many children it actually has, and a prerequisite reference is only
meaningful once every node in the tree exists. In an N-Tier design those
rules tend to live in a service class that trusts whatever data it's handed;
here they live *inside* the entities themselves, enforced through factory
methods and private setters, so it's structurally impossible to construct an
invalid `Group` or `Step` in the first place — not just unlikely, impossible.
That also makes the domain logic trivially unit-testable without a database
(see *Testing* below), and keeps the validation/simulation rules — the actual
hard part of this problem — decoupled from EF Core, HTTP, or any framework
concern.

The solution is split into four projects:

```
ProgramDesigner.Domain          →  Entities, Value Objects, the LearningProgram
                                    Aggregate Root, Domain Services (validation +
                                    simulation), Repository interface
ProgramDesigner.Infrastructure  →  EF Core DbContext, entity configurations,
                                    the Repository implementation, DB seeding
ProgramDesigner.Application     →  DTOs, the Program orchestration service,
                                    GeneralResult<T> (Result Pattern), mapping
ProgramDesigner.API             →  Controllers, Program.cs composition root,
                                    Scalar docs
ProgramDesigner.Tests           →  xUnit tests for the Domain Services
                                    (no database involved)
```

Dependency direction is one-way and points inward: `API → Application → Domain
← Infrastructure`. The Domain project has zero dependencies on EF Core or any
other package — Infrastructure depends on Domain, not the other way round.
Each layer registers its own services through a `ServicesExtension`, so
`Program.cs` stays a thin composition root.

Key patterns used throughout: **Aggregate Root** (`LearningProgram` is the
only entry point into a program's tree — `Group`/`Step` are never fetched or
saved on their own), **Rich Domain Model** (invariants like "no self-
referencing prerequisite" or "pick N of M can't exceed the child count" are
enforced *inside* the entities via factory methods and private setters, not
re-checked by every caller), **Domain Services** for logic that spans the
whole tree (validation, simulation), **Repository Pattern**, and the
**General Result Pattern** (`GeneralResult<T>` — every Application-layer
operation returns `Success`, `Data`, and a list of `Error`s instead of
throwing for expected failure cases).

---
## Trade-offs

Some decisions here were about balancing correctness, complexity, and time. Worth naming explicitly:

**DDD over a simpler layered approach.**  A rich Domain Model adds real overhead: private constructors, factory methods, and EF Core's backing-field access mode are all more code than a plain anemic model with public setters and a DbContext doing the validating. For a CRUD-heavy API this would be over-engineering. It was worth it here because the domain has genuine invariants to protect (self-reference, nested prerequisites, choice-count consistency) — the extra structure buys guarantees, not just style.

**Multiple prerequisites as a join table, not a richer graph structure.**  A self-referencing many-to-many table is the simplest model that satisfies AND-semantics prerequisites.

**Impossible prerequisites rejected at creation , not left for validate to catch.**  This means Create does more work than a bare insert (it runs the full validation pass before saving). The trade-off is a slightly slower write path in exchange for a guarantee that nothing invalid ever reaches the database — reads (GET, validate) never need to handle "this program is fundamentally broken" as a possible state.

## Data Model

### Why Step and Group share one table (TPH)

A `Group` must be able to hold a mixed, ordered list of `Step`s and other
`Group`s, nested to any depth. To represent that faithfully, `Step` and
`Group` both inherit from an abstract `ProgramItem` Domain entity, mapped
with EF Core's **Table-Per-Hierarchy (TPH)** strategy — one physical table
(`ProgramItems`) with a discriminator column (`ItemType`). This is what makes
a `Group.Children` collection able to hold both types together, in one true
order, at any depth, without a fixed schema per level.

```
ProgramItems
├── Id, Name, Order, ParentGroupId (FK, self)
├── ItemType (discriminator: "Step" | "Group")
├── StepType             (Step only)
└── RuleType, ChoiceCount   (Group only — RuleType/ChoiceCount are actually a
                             single GroupRule Value Object, stored as an
                             EF Core owned type on the same row)

ProgramItemPrerequisites   (join table)
├── ProgramItemId   (the item that has the prerequisite)
└── PrerequisiteId  (the item it depends on)
```

- **Prerequisites support more than one per item **: an item
  only unlocks once *every* listed prerequisite is complete. This is modelled
  as a self-referencing many-to-many relationship
  (`ProgramItem.Prerequisites`) backed by the `ProgramItemPrerequisites` join
  table, rather than a single nullable `PrerequisiteItemId` FK.
- Both self-referencing relationships (`ParentGroupId` and the
  `ProgramItemPrerequisites` join) use `ON DELETE Restrict`. SQL Server
  rejects `CASCADE` on multiple self-references into the same table
  ("may cause cycles or multiple cascade paths"), and it also gives explicit
  control over deletion behavior at the Application layer if a delete
  endpoint is added later.
- EF Core reads/writes `Name`, `Order`, `Rule`, etc. straight through their
  **backing fields** (`PropertyAccessMode.Field`), never through a public
  setter — the Domain model keeps its constructors private and its setters
  protected/internal, exactly as it would without EF Core in the picture.
  Every entity also has a private parameterless constructor used only by EF
  Core for materialization.
- `LearningProgram` is the Aggregate Root: a thin wrapper (`Id`, `Name`,
  `CreatedAt`, `RootGroup`) that gives a program its own identity, separate
  from "being a Group," and is the only type the Repository exposes.

### Why the create request uses temporary `Key`/`PrerequisiteKeys`

When a program is created, none of its items have real database IDs yet.
So the request format lets the caller assign a temporary, request-scoped
`Key` to every node, and reference one or more other nodes' `Key`s via
`PrerequisiteKeys` (a list — supports the AND-semantics multiple
prerequisites described above). The Application layer resolves these into
real Domain entity references in memory, and EF Core assigns the actual IDs
once the whole tree is saved in one transaction.

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
   dotnet run --project ProgramDesigner.API
   ```
   Pending migrations are applied automatically on startup — there's no
   separate `dotnet ef database update` step to remember.
4. On first run, the database is automatically seeded with the full
   **Computer Science** scenario from the challenge brief (program `Id: 1`).
5. Scalar API Reference opens automatically in your default browser. If it
   doesn't, check the console output after `dotnet run` for a line like
   `Now listening on: http://localhost:5219`, and navigate to that same
   address with `/scalar/v1` appended (e.g. `http://localhost:5219/scalar/v1`).

Use Scalar to explore and test all available endpoints.

Total time from clone to a running, seeded API: under 5 minutes.
---

## API Reference

### `POST /programs`

Creates a program from a JSON tree. `Key` is a temporary, request-only
identifier used to wire up prerequisites within the same request;
`PrerequisiteKeys` references one or more other nodes' `Key`s anywhere in the
tree — every one of them must be completed before the item unlocks.

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
          { "key": "s1", "name": "Introduction to Computing", "itemType": "Step", "stepType": "Attend Session", "prerequisiteKeys": [] }
        ]
      },
      {
        "key": "major",
        "name": "Major",
        "itemType": "Group",
        "ruleType": "Choice",
        "choiceCount": 1,
        "prerequisiteKeys": ["foundations"],
        "children": []
      }
    ]
  }
}
```

Returns `201 Created` with the saved program (real IDs assigned).

Returns `400 Bad Request` with a list of errors if:
- the request shape is invalid (unknown `itemType`/`ruleType`, a `Choice`
  group's `choiceCount` doesn't fit its children, a duplicate or missing
  `Key`, a `PrerequisiteKeys` entry that doesn't resolve within the request,
  or an item listing itself as its own prerequisite), **or**
- the tree is structurally impossible once assembled — a direct or indirect
  prerequisite cycle, a prerequisite on something nested inside itself, or a
  prerequisite on something that appears later in the program.

Returns `409 Conflict` if a program with the same `Name` already exists.

**Impossible prerequisites are rejected right here, at creation** — they can
never become valid later, so there's no reason to let them into the
database. **Reachability warnings are not blocking**: a prerequisite that
sits inside a `Choice` branch a participant might never pick is a valid,
saved program; call `POST /programs/{id}/validate` at any time afterward to
see those warnings (e.g. after reviewing an older program, or after a future
edit endpoint changes its structure).

### `GET /programs/{id}`

Returns the full nested structure of a program, with real IDs throughout, no
matter how deeply it's nested — the Repository loads the tree breadth-first,
level by level, rather than a fixed-depth `Include()` chain. Returns `404` if
the program doesn't exist.

### `POST /programs/{id}/validate`

Runs validation against the saved program and returns:

```json
{
  "isValid": true,
  "impossiblePrerequisites": [],
  "reachabilityWarnings": []
}
```

`isValid` is `false` only when `impossiblePrerequisites` is non-empty.
Reachability warnings never affect validity. Returns `404` if the program
doesn't exist.

### `POST /programs/{id}/simulate`

Given a participant's completed steps and the branches they picked in
choice groups, returns the current state of every item in the tree. Nothing
here is persisted — it's a pure read against the saved structure.

```json
{
  "choices": { "3": [6] },
  "completedItemIds": [4, 5]
}
```

`choices` maps a Choice group's `Id` to the `Id`s of the children the
participant selected. `completedItemIds` lists the `Id`s of Steps already
done. Both are optional — an empty body simulates a participant who hasn't
started or decided anything yet. Returns `404` if the program doesn't exist.

```json
{
  "items": [
    { "itemId": 6, "itemName": "AI", "itemType": "Group", "status": "Unlocked", "reason": null },
    { "itemId": 7, "itemName": "IT", "itemType": "Group", "status": "Excluded", "reason": "This branch was not selected in the participant's choices, so it can never be completed." },
    { "itemId": 9, "itemName": "Final Capstone", "itemType": "Step", "status": "Blocked", "reason": "Waiting on: Major" }
  ]
}
```

Every item gets one of four statuses:
- **Complete** — a Step in `completedItemIds`, or a Group whose rule is
  already satisfied (all children for `InOrder`, enough completed children
  for `Choice`).
- **Unlocked** — can be attempted right now.
- **Blocked** — waiting on one or more prerequisites (the `reason` lists
  every one still incomplete), an earlier sibling in an `InOrder` group, or a
  parent group that isn't unlocked yet (status is inherited down the tree).
- **Excluded** — sits inside a choice branch the participant didn't pick;
  can never be completed.

---

## Validation Logic

Both Domain Services (`PrerequisiteValidationService` and
`ProgramSimulationService`) operate directly on the in-memory object graph —
`Group.Children` and `ProgramItem.Prerequisites` are real references once the
Aggregate is loaded (or freshly built), so no ID-based lookups or rebuilding
are needed at all. A single depth-first pass over the tree assigns each node
an **enter/exit order** (an Euler-tour-style interval), which turns "is X
inside Y?" and "does X appear after Y?" into O(1) integer comparisons.

**Impossible prerequisites** (rejected — `IsValid: false`) are detected per
prerequisite edge (an item can have several; each is checked independently),
in order:
1. The item points at itself — actually unreachable in practice, since
   `ProgramItem.AddPrerequisite` rejects this the moment it would happen,
   before the item can ever be saved. The check still exists in the
   validation service as a defensive fallback.
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

`PrerequisiteValidationServiceTests` and `ProgramSimulationServiceTests` run
entirely against the Domain layer — no database, no HTTP, trees are built by
hand in-memory through the same `Group.Create`/`Step.Create`/`AddChild`/
`AddPrerequisite` API the rest of the application uses. Together they cover:

- The full Computer Science scenario validates with no errors or warnings.
- A direct and an indirect prerequisite cycle are both rejected.
- A prerequisite pointing at itself is rejected at the Domain level
  (`AddPrerequisite` throws immediately).
- A prerequisite inside an unguaranteed choice branch produces a warning,
  not a rejection.
- An item with multiple prerequisites is rejected if *any* of them is
  impossible, and a valid one doesn't hide a broken one.
- A fresh start where only the first step is unlocked, an undecided choice
  group where no branch is excluded yet, a decided choice where the other
  branches become excluded, a fully completed track that unlocks what comes
  after it, and an item with multiple incomplete prerequisites lists every
  one of them in its `Blocked` reason.

---

## AI Tool Usage

During development, I occasionally used AI as a technical assistant for 
discussing design ideas,exploring alternative solutions, and troubleshooting 
specific issues. However, the project architecture, technical decisions, validation 
logic, debugging, and testing were designed, written, and verified by me.
