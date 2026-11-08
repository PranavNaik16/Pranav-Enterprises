namespace PranavEnterprises.Domain.Enums;

public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Terminated = 3
}

public enum ProjectStatus
{
    Planning = 1,
    InProgress = 2,
    OnHold = 3,
    Completed = 4,
    Cancelled = 5
}

public enum PriorityLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum TaskItemStatus
{
    Todo = 1,
    InProgress = 2,
    InReview = 3,
    Completed = 4
}
