# BigBall Web - Refactoring Guidelines & Component Architecture

**Version:** 1.0  
**Last Updated:** 2026-04-26  
**Status:** Active - Applied to Predict.razor as proof-of-concept

---

## Core Principle

Refactor from **inline-style, primitive-HTML pages** to **component-based architecture with encapsulated styles**. Goal: Create reusable, maintainable components suitable for both web (Blazor WebAssembly) and native (MAUI) ports.

---

## Pattern Overview

### Before (Inline Styles)

```razor
<div style="padding:32px 40px;display:grid;grid-template-columns:1fr auto 1fr;align-items:center;gap:32px;">
    <div style="display:flex;flex-direction:column;align-items:center;gap:14px;">
        <Flag Code="@match.HomeCode" Size="72" />
        <div style="font-family:var(--font-display);font-size:20px;font-weight:600;letter-spacing:-0.02em;">@match.HomeCode</div>
    </div>
    <!-- More nested inline styles... -->
</div>
```

### After (Component-Based)

```razor
<MatchCard HomeCode="@match.HomeCode"
           AwayCode="@match.AwayCode"
           HomePrediction="@VM.HomePrediction"
           AwayPrediction="@VM.AwayPrediction"
           IsLocked="@VM.IsLocked"
           IsBusy="@VM.IsBusy"
           HomeReferenceScore="@match.ReferenceHome"
           AwayReferenceScore="@match.ReferenceAway"
           Preview="@VM.Preview"
           HomePredictionChanged="v => VM.HomePrediction = v"
           AwayPredictionChanged="v => VM.AwayPrediction = v" />
```

---

## Component Design Principles

### 1. **Composability**

- Each component should have a single, clear responsibility
- Components should be composable into larger UI structures
- Avoid monolithic components that do too much

**Example Structure:**

```
<PageComponent>
  <HeaderComponent />
  <LayoutContainer>
    <CardComponent>
      <DisplayComponent />
      <InputComponent />
    </CardComponent>
    <SidebarLayout>
      <InfoCard />
      <ActionButton />
    </SidebarLayout>
  </LayoutContainer>
</PageComponent>
```

### 2. **Encapsulation**

- Move all inline `style` attributes to component `<style>` blocks
- CSS classes should be scoped to the component
- Never use global inline styles in page markup

**Good:**

```razor
<div class="match-display">
    <!-- markup -->
</div>

<style>
    .match-display {
        padding: 32px 40px;
        display: grid;
        grid-template-columns: 1fr auto 1fr;
        align-items: center;
        gap: 32px;
    }
</style>
```

**Bad:**

```razor
<div style="padding:32px 40px;display:grid;grid-template-columns:1fr auto 1fr;align-items:center;gap:32px;">
```

### 3. **Reusability**

- Design components for reuse across multiple pages
- Accept configuration through parameters, not hardcoding
- Use `EventCallback` for child-to-parent communication

**Example:** MatchDisplay component can be reused in:

- Predict.razor (prediction page)
- Calendar.razor (calendar view)
- Match results view (future)
- Mobile views

### 4. **Parameter Design**

- Use `[Parameter]` for one-way data binding (parent → child)
- Use `[Parameter] public EventCallback<T>` for events (child → parent)
- Keep parameters semantically meaningful (e.g., `IsLocked` not `Disabled`)
- Document nullable parameters clearly

**Example:**

```csharp
[Parameter] public string HomeCode { get; set; } = "";                    // Required
[Parameter] public int? HomeReferenceScore { get; set; }                 // Optional (nullable)
[Parameter] public bool IsLocked { get; set; }                           // Boolean flag
[Parameter] public EventCallback<int> HomePredictionChanged { get; set; } // Event callback
```

### 5. **Styling Guidelines**

- Use CSS custom properties (`--bg`, `--fg`, `--brand-strong`, etc.) from `tokens-desktop.css`
- Leverage existing CSS classes (`.bb-dcard`, `.bb-btn`, `.bb-chip`, etc.)
- Avoid inventing new classes; reuse design tokens
- Keep component-specific CSS minimal

**Reusable CSS Classes (from tokens-desktop.css):**

- `.bb-dcard` — Card container with elevation and borders
- `.bb-btn`, `.bb-btn-primary` — Button styling
- `.bb-chip`, `.bb-chip live` — Badge/chip styling
- `.bb-page-h`, `.bb-page-title`, `.bb-page-eyebrow`, `.bb-page-sub` — Page headers
- `.bb-table`, `.bb-tier-table` — Table layouts
- `.bb-error` — Error message styling
- `.bb-field-label` — Form label styling

---

## Component Creation Checklist

When creating a new component, follow this checklist:

- [ ] **Identify the semantic boundary** — What logical UI unit does this component represent?
- [ ] **Name meaningfully** — Use descriptive PascalCase names (e.g., `MatchDisplay`, not `Card`)
- [ ] **Define parameters** — List all required and optional inputs
- [ ] **Define events** — List all callbacks/outputs
- [ ] **Implement markup** — Write clean HTML using existing CSS classes
- [ ] **Encapsulate styles** — Move all styles to `<style>` block
- [ ] **Test composition** — Verify it works as a child of parent components
- [ ] **Document usage** — Add inline comments explaining non-obvious parameters

**Minimal Component Template:**

```razor
<div class="component-name">
    <!-- markup -->
</div>

@code {
    [Parameter] public string RequiredParam { get; set; } = "";
    [Parameter] public int? OptionalParam { get; set; }
    [Parameter] public EventCallback<string> OnAction { get; set; }
}

<style>
    .component-name {
        /* styles */
    }
</style>
```

---

## Refactoring Page Flow

### Step 1: Analyze

- Read through the page and identify visual/functional sections
- Look for repeated patterns (cards, tables, form inputs)
- Identify stateless (display) vs. stateful (logic) sections

### Step 2: Design Component Hierarchy

- Sketch the component tree on paper or in markdown
- Identify container vs. leaf components
- Plan parameter passing and event flow

**Example for Predict.razor:**

```
Predict (page, stateful)
├── MatchHeader (stateless display)
├── MatchCard (container)
│   ├── MatchDisplay (stateless display)
│   └── PredictionInput (stateful input)
└── PredictionLayout (container)
    ├── ScoringRulesCard (stateless display)
    ├── ErrorAlert (conditional display)
    └── SavePredictionButton (stateful button)
```

### Step 3: Extract Utilities

- Identify helper functions used in the page
- Move to a shared `.cs` file in `Shared/UI/`
- Make available for reuse

**Example:** `TimeFormatting.cs`

```csharp
public static class TimeFormatting
{
    public static string FormatCountdown(int seconds) { /* ... */ }
}
```

### Step 4: Create Components (Bottom-Up)

- Start with leaf components (display-only, no logic)
- Move to container components (composition)
- End with page refactoring

**Order for Predict.razor:**

1. MatchDisplay (pure display)
2. MatchHeader (pure display)
3. ErrorAlert (conditional display)
4. SavePredictionButton (button logic)
5. ScoringRulesCard (static display)
6. PredictionInput (complex input)
7. PredictionLayout (container)
8. MatchCard (container)
9. Predict.razor (refactor to use all above)

### Step 5: Refactor the Page

- Replace inline HTML sections with component usages
- Remove inline styles from markup
- Keep only page-specific layout CSS
- Test functionality end-to-end

### Step 6: Verify

- [ ] No inline `style` attributes in page markup
- [ ] All CSS in component `<style>` blocks
- [ ] Components are reusable (not page-specific)
- [ ] All event callbacks wired correctly
- [ ] Build succeeds with zero warnings
- [ ] Visual output matches original
- [ ] Functionality works (form submission, state changes, etc.)

---

## Common Patterns

### Pattern 1: Display-Only Component

Used for read-only UI sections (cards, text, images).

```razor
<!-- MatchDisplay.razor -->
<div class="match-display">
    <div class="team-column">
        <Flag Code="@HomeCode" Size="72" />
        <div class="team-code">@HomeCode</div>
    </div>
    <div class="vs-divider">vs</div>
    <div class="team-column">
        <Flag Code="@AwayCode" Size="72" />
        <div class="team-code">@AwayCode</div>
    </div>
</div>

@code {
    [Parameter] public string HomeCode { get; set; } = "";
    [Parameter] public string AwayCode { get; set; } = "";
}

<style>
    .match-display { /* styles */ }
    .team-column { /* styles */ }
    .vs-divider { /* styles */ }
    .team-code { /* styles */ }
</style>
```

### Pattern 2: Input Component with EventCallback

Used for user input that bubbles to parent state.

```razor
<!-- PredictionInput.razor -->
<div class="prediction-input-section">
    <div class="prediction-label">Seu palpite</div>
    <div class="prediction-inputs">
        <ScoreStepper Value="@HomePrediction"
                      ValueChanged="v => HomePredictionChanged.InvokeAsync(v)"
                      Disabled="@(IsLocked || IsBusy)" />
        <div class="vs-operator">×</div>
        <ScoreStepper Value="@AwayPrediction"
                      ValueChanged="v => AwayPredictionChanged.InvokeAsync(v)"
                      Disabled="@(IsLocked || IsBusy)" />
    </div>
</div>

@code {
    [Parameter] public int HomePrediction { get; set; }
    [Parameter] public int AwayPrediction { get; set; }
    [Parameter] public bool IsLocked { get; set; }
    [Parameter] public bool IsBusy { get; set; }

    [Parameter] public EventCallback<int> HomePredictionChanged { get; set; }
    [Parameter] public EventCallback<int> AwayPredictionChanged { get; set; }
}

<style>
    .prediction-input-section { /* styles */ }
    /* ... */
</style>
```

### Pattern 3: Container Component

Used to compose smaller components into larger structures.

```razor
<!-- MatchCard.razor -->
<div class="bb-dcard" style="padding:0;overflow:hidden;">
    <div class="match-card-display-section">
        <MatchDisplay HomeCode="@HomeCode" AwayCode="@AwayCode" />
    </div>
    <div class="match-card-input-section">
        <PredictionInput HomePrediction="@HomePrediction"
                         AwayPrediction="@AwayPrediction"
                         IsLocked="@IsLocked"
                         IsBusy="@IsBusy"
                         HomeReferenceScore="@HomeReferenceScore"
                         AwayReferenceScore="@AwayReferenceScore"
                         Preview="@Preview"
                         HomePredictionChanged="@HomePredictionChanged"
                         AwayPredictionChanged="@AwayPredictionChanged" />
    </div>
</div>

@code {
    [Parameter] public string HomeCode { get; set; } = "";
    [Parameter] public string AwayCode { get; set; } = "";
    /* ... re-export all child parameters ... */
}

<style>
    .match-card-display-section { border-bottom: 1px solid var(--line); }
</style>
```

### Pattern 4: Conditional Component

Used for conditional rendering (errors, alerts, etc.).

```razor
<!-- ErrorAlert.razor -->
@if (!string.IsNullOrEmpty(Message))
{
    <div class="bb-error">@Message</div>
}

@code {
    [Parameter] public string? Message { get; set; }
}
```

### Pattern 5: Layout Wrapper

Used for layout structure without semantic meaning.

```razor
<!-- PredictionLayout.razor -->
<div class="prediction-layout">
    @ChildContent
</div>

@code {
    [Parameter] public RenderFragment? ChildContent { get; set; }
}

<style>
    .prediction-layout {
        display: flex;
        flex-direction: column;
        gap: 20px;
    }
</style>
```

---

## Type Safety Notes

### Handling Dynamic Objects

When passing objects with unknown compile-time types (e.g., `VM.Preview` from ViewModel):

```csharp
// Use dynamic for flexible property access
[Parameter] public dynamic? Preview { get; set; }
```

In markup, access properties directly:

```razor
@if (Preview is { } preview)
{
    <strong>@preview.Total pts</strong>
    <span>@preview.Tier</span>
}
```

### Handling Nullable Values

When dealing with optional parameters:

```csharp
// Use nullable types to explicitly represent absence
[Parameter] public int? HomeReferenceScore { get; set; }

// In usage
HomeReferenceScore="@(match.ReferenceHome ?? 0)"
// OR
HomeReferenceScore="@match.ReferenceHome"  // Pass nullable as-is
```

---

## Navigation & Sidebar Integration

When creating new pages or refactoring, ensure proper sidebar navigation:

1. **Add link to Sidebar.razor:**

   ```razor
   <NavLink href="/path/to/page" class="nav-item">
       <Icon Path="@IconPaths.Icon" Size="20" />
       Label
   </NavLink>
   ```

2. **Add active state tracking:**

   ```csharp
   [Parameter] public string Active { get; set; } = "";

   // In markup
   class="nav-section @(Active == "section-key" ? "active" : "")"
   ```

3. **Update page route:**
   ```razor
   @page "/route/path/{parameter:guid}"
   ```

---

## Pages Ready for Refactoring

| Page             | Status       | Priority | Estimated Components                                      |
| ---------------- | ------------ | -------- | --------------------------------------------------------- |
| Predict.razor    | ✅ Completed | N/A      | 9 components created                                      |
| Calendar.razor   | ⏳ Pending   | High     | ~8-10 components (date picker, match list, detail panel)  |
| Home.razor       | ⏳ Pending   | High     | ~6-8 components (hero, table, stats)                      |
| PoolDetail.razor | ⏳ Pending   | High     | ~5-7 components (ranking table, tie alert, position card) |
| Profile.razor    | ⏳ Pending   | Medium   | ~7-9 components (hero, stats, activity table, settings)   |

---

## Anti-Patterns to Avoid

❌ **Don't:**

- Create components for every single HTML element
- Use inline styles in page markup
- Hardcode values in components (accept as parameters)
- Create overly generic component names (Wrapper, Box, Container)
- Mix display and business logic in a single component
- Export private helper methods from components
- Create components that are only used once (unless they're reusable later)

✅ **Do:**

- Group related elements into semantically meaningful components
- Encapsulate all styling in component `<style>` blocks
- Use parameters to configure components
- Name components after their semantic purpose (MatchDisplay, ErrorAlert, etc.)
- Separate concerns (display components vs. logic components)
- Keep components focused and testable
- Design with reusability in mind from the start

---

## Tools & Utilities Location

All reusable utilities go in `src/BigBall.Web/Shared/UI/`:

- **TimeFormatting.cs** — Time formatting utilities
- **[Future]** — Form validation utilities
- **[Future]** — String formatting utilities
- **[Future]** — Date formatting utilities

---

## CSS Token System

**Location:** `wwwroot/css/tokens-desktop.css`

**Available Custom Properties:**

```css
/* Colors */
--bg, --bg-elev, --bg-elev-2
--fg, --fg-2, --fg-3
--line, --line-strong
--brand-strong, --brand-ink
--success, --warning, --error
--chip-bg

/* Typography */
--font-display, --font-sans

/* Sizing */
/* Standard padding values: 8px, 12px, 16px, 20px, 24px, 32px, 40px, etc. */

/* Border Radius */
/* 6px, 8px, 10px, 12px, 16px */
```

Use these tokens consistently across all components.

---

## Example: Complete Refactoring (Predict.razor)

**Files Created:**

1. `Shared/UI/TimeFormatting.cs`
2. `Shared/UI/MatchHeader.razor`
3. `Shared/UI/MatchDisplay.razor`
4. `Shared/UI/PredictionInput.razor`
5. `Shared/UI/MatchCard.razor`
6. `Shared/UI/ScoringRulesCard.razor`
7. `Shared/UI/ErrorAlert.razor`
8. `Shared/UI/SavePredictionButton.razor`
9. `Shared/UI/PredictionLayout.razor`

**Files Modified:**

- `Pages/Predict.razor` (refactored to use above components)

**Result:**

- Predict.razor reduced from ~140 lines with inline styles to ~60 lines of clean component composition
- 9 reusable components created for future use
- All CSS encapsulated and maintainable
- Pattern established for other pages

---

## Review Checklist (Pre-Commit)

Before committing refactoring changes:

- [ ] All new components created and tested
- [ ] Page markup refactored to use components
- [ ] No inline `style` attributes in page or component markup
- [ ] All CSS in component `<style>` blocks
- [ ] Component parameters documented
- [ ] EventCallback bindings verified
- [ ] Build succeeds with zero warnings/errors
- [ ] Visual output matches original design
- [ ] Functionality tested end-to-end
- [ ] Components are truly reusable (not page-specific)
- [ ] Sidebar navigation updated if needed
- [ ] Commit message describes component architecture pattern

---

## Future Enhancements

- [ ] Create a component library documentation site
- [ ] Add component examples/stories
- [ ] Create component test utilities
- [ ] Establish component versioning strategy
- [ ] Build design system documentation
- [ ] Create MAUI component equivalents

---

**Document Version History:**

- v1.0 (2026-04-26) — Initial guidelines based on Predict.razor refactoring
