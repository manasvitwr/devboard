namespace DevBoard.Models
{
    public enum TicketType
    {
        Feature = 0,
        Bug = 1,
        QADebt = 2,
        Chore = 3
    }

    public enum Status
    {
        Todo = 0,
        InProgress = 1,
        Done = 2
    }

    public enum Priority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    public enum TestEffort
    {
        Small = 0,
        Medium = 1,
        High = 2
    }
}
