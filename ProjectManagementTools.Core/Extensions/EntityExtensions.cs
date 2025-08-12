using ProjectManagementTools.Core.Entities;
using ProjectManagementTools.Core.Entities.Projects;
using ProjectManagementTools.Core.Enums;

namespace ProjectManagementTools.Core.Extensions
{
    public static class EntityExtensions
    {
        // Project Extensions
        public static bool IsOverdue(this Project project)
        {
            return project.EndDate < DateTime.UtcNow &&
                   project.Status != ProjectStatus.Completed &&
                   project.Status != ProjectStatus.Cancelled;
        }

        public static decimal CalculateProgress(this Project project)
        {
            if (project.Tasks == null || !project.Tasks.Any())
                return 0;

            var totalTasks = project.Tasks.Count;
            var completedTasks = project.Tasks.Count(t => t.Status == Enums.TaskStatus.Done);

            return totalTasks > 0 ? (decimal)completedTasks / totalTasks * 100 : 0;
        }

        // Task Extensions
        public static bool IsOverdue(this TaskItem task)
        {
            return task.EndDate < DateTime.UtcNow &&
                   task.Status != Enums.TaskStatus.Done &&
                   task.Status != Enums.TaskStatus.Cancelled;
        }

        public static bool IsInProgress(this TaskItem task)
        {
            return task.Status == Enums.TaskStatus.InProgress;
        }

        public static bool IsCompleted(this TaskItem task)
        {
            return task.Status == Enums.TaskStatus.Done;
        }

        public static TimeSpan GetRemainingTime(this TaskItem task)
        {
            return task.EndDate > DateTime.UtcNow ?
                   task.EndDate - DateTime.UtcNow :
                   TimeSpan.Zero;
        }

        // Sprint Extensions
        public static bool IsActive(this Sprint sprint)
        {
            return sprint.Status == SprintStatus.Active;
        }

        public static bool IsOverdue(this Sprint sprint)
        {
            return sprint.EndDate < DateTime.UtcNow &&
                   sprint.Status == SprintStatus.Active;
        }

        public static decimal CalculateVelocity(this Sprint sprint)
        {
            if (sprint.Status != SprintStatus.Completed)
                return 0;

            var totalDays = (sprint.EndDate - sprint.StartDate).Days + 1;
            return totalDays > 0 ? (decimal)sprint.CompletedStoryPoints / totalDays : 0;
        }

        // Time Log Extensions
        public static decimal CalculateCost(this TimeLog timeLog)
        {
            return timeLog.HourlyRate.HasValue ?
                   timeLog.HoursWorked * timeLog.HourlyRate.Value : 0;
        }

        public static bool IsCurrentlyRunning(this TimeLog timeLog)
        {
            return timeLog.IsRunning && !timeLog.EndTime.HasValue;
        }
    }
}
