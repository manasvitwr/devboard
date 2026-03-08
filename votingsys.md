 here is the official spec for the **DevBoard Priority & Health Engine**.

---

## 1. The Core Philosophy: "Up" Always Means "Urgent"

To prevent the semantic confusion we discussed, we are moving away from "Sentiment" (Do I like this?) to "Signal" (Does this need eyes?).

* **Ticket Level:** Upvoting = **Individual Priority Boost**.
* **Category Level:** Upvoting = **Systemic Risk Signal**.
* **Module Level:** **Visual Aggregation** (Calculated, not voted).

---

## 2. Voting Mechanics & User Personas

We will implement **Weighted Voting** to ensure a Junior Dev can’t accidentally tank a module's health without oversight from a Lead or Admin.
(edit this and make this acc to our users ie the qa dev admin and stakeholder etc, this is just a sample)
| Persona | Vote Weight ($W_u$) | Intent |
| --- | --- | --- |
| **Admin** | 10 | Strategic Oversight / Emergency Flagging |
| **Lead** | 5 | Technical Health / Architectural Risk |
| **Developer** | 1 | Day-to-day friction / Bug reporting |

### A. The Ticket "Boost" (Kanban Board)

* **Action:** Clicking the `^` (Boost) icon on a card.
* **Limit:** 1 boost per user per ticket.
* **Logic:** $TicketPriority = BasePriority + \sum(Vote \times W_u)$.

### B. The Category "Signal" (Voting Dashboard)

* **Action:** Clicking the `Down` (Thumbs Down) on a Category row ("This area is unstable").
* **Logic:** This increases the **Stress Index** of the category. Higher stress = lower health. (Conversely, an `Up` vote means "Stable" and decreases stress).

---

## 3. The "Health Engine" Formulae

This is the math that powers your **Module Voting Dashboard** visualization.

### Step 1: Category Stress Score ($S_c$)

We calculate how much "pain" a category is in. We use the **"Gravity Well"** logic where widely-boosted tickets weigh down the category automatically.


$$S_c = \left(\sum \text{Net Category Stress Votes (-1 for Up, +1 for Down)} \times W_u\right) + \left( \sum \text{Ticket Boosts} \times 0.2 \right)$$

### 2. The Link: How Ticket Votes *Do* Effect the Score

To make the system "Better than Jira," we implement a **"Gravity Well"** logic. When a ticket gets a massive amount of "Boosts" (Upvotes), it starts to "weigh down" the category it belongs to.

**The Modified Formula:**
Instead of the Health Score ignoring tickets, we add a **Weighted Ticket Penalty** ($P_t$):

$$S_c = (\text{Category Votes} \times W_u) + \left( \sum \text{Ticket Boosts} \times 0.2 \right)$$
*(Note: Category Votes are -1 for Up/Stable, +1 for Down/Unstable)*

* **Why 0.2?** Because 5 people boosting a single ticket is roughly equal to 1 person flagging the entire Category as "Broken."
* **The Result:** If a category has 10 tickets and everyone is "Boosting" them, the Module Health will automatically turn **Red**, even if nobody has voted on the Category itself.

---

### 3. Why Keep Them Separate at All?

If they both affect the score, why have both?

1. **Precision:** A manager needs to know *why* a module is Red.
* If it’s red because of **Category Votes**, it’s a sentiment/trust issue (The team thinks the code is messy).
* If it’s red because of **Ticket Votes**, it’s a workload issue (There are too many specific bugs).

2. **The "Priority Boost":** A ticket upvote moves that specific card to the top of the "To Do" list. A category vote does not move individual cards; it just changes the color of the Dashboard.

---

### 4. Summary of the "Unified" Flow

1. **Dev votes for a Ticket:** The ticket moves up the Kanban board. The Category "Stress" increases slightly.
2. **Lead votes for a Category:** The ticket stays where it is, but the **Module Health Bar** on the main Projects page drops significantly.
3. **The Engine:** Aggregates both. If the total "Pain" (Tickets + Votes) is too high, the project is marked "At Risk".

---

## 5. Feature: The "Automatic Triage" (Anti-Jira USP)

This is where your tool beats Jira. Instead of a manager manually moving tickets to "High Priority," the **Health Engine** does it.

* **Logic:** If a Category's $S_c$ exceeds a threshold (e.g., 50), all **new** tickets created in that category are automatically tagged as **"High Priority"** and moved to the top of the "To Do" column on the Kanban Board.
* **Visual:** The Category row in the Dashboard flashes or shows a "High Risk" tag.

---

## 5. Implementation Roadmap (WebForms Layout)

### UI Changes

1. **Kanban Cards:** Replace the current green/red arrows with a single "Flame" or "Arrow Up" icon for "Boost".
2. **Dashboard Rows:** Use the "Reddit-style" layout but label the Up button as **"Signal Issue"**.
3. **Recent Activity:** On the right-hand side, show a feed: *"Admin boosted 'Authentication' stress level"*.

### Database Schema Update

* **Users Table:** Add `UserWeight (INT)`.
* **Votes Table:** Add `VoteTarget ('Ticket' or 'Category')`.
* **Categories Table:** Add `StressScore (DECIMAL)`.

-----------------------------
added logic for modifications 

### 2. The Link: How Ticket Votes *Do* Effect the Score

To make the system "Better than Jira," we implement a **"Gravity Well"** logic. When a ticket gets a massive amount of "Boosts" (Upvotes), it starts to "weigh down" the category it belongs to.

**The Modified Formula:**
Instead of the Health Score ignoring tickets, we add a **Weighted Ticket Penalty** ($P_t$):

$$S_c = (\text{Category Votes} \times W_u) + \left( \sum \text{Ticket Boosts} \times 0.2 \right)$$

* **Why 0.2?** Because 5 people boosting a single ticket is roughly equal to 1 person flagging the entire Category as "Broken."
* **The Result:** If a category has 10 tickets and everyone is "Boosting" them, the Module Health will automatically turn **Red**, even if nobody has voted on the Category itself.

---

### 3. Why Keep Them Separate at All?

If they both affect the score, why have both?

1. **Precision:** A manager needs to know *why* a module is Red.
* If it’s red because of **Category Votes**, it’s a sentiment/trust issue (The team thinks the code is messy).
* If it’s red because of **Ticket Votes**, it’s a workload issue (There are too many specific bugs).


2. **The "Priority Boost":** A ticket upvote moves that specific card to the top of the "To Do" list. A category vote does not move individual cards; it just changes the color of the Dashboard.

---

### 4. Summary of the "Unified" Flow

1. **Dev votes for a Ticket:** The ticket moves up the Kanban board. The Category "Stress" increases slightly.
2. **Lead votes for a Category:** The ticket stays where it is, but the **Module Health Bar** on the main Projects page drops significantly.
3. **The Engine:** Aggregates both. If the total "Pain" (Tickets + Votes) is too high, the project is marked "At Risk".

