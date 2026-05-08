namespace AlphaSquad.Shared.DTOs.Workouts;

public record WorkoutResponse(
    Guid Id, 
    string Name, 
    string? Description, 
    string? Goal, 
    bool IsActive, 
    DateTime CreatedAt
);

public record WorkoutCreateRequest(
    string Name, 
    string? Description, 
    string? Goal
);

public record WorkoutUpdateRequest(
    string Name, 
    string? Description, 
    string? Goal, 
    bool IsActive
);

public record WorkoutExerciseResponse(
    Guid ExerciseId, 
    string ExerciseName, 
    string? MediaUrl, 
    int Order, 
    int Sets, 
    string Reps, 
    int? RestTime, 
    string? Notes
);

public record WorkoutDetailsResponse(
    WorkoutResponse Workout, 
    List<WorkoutExerciseResponse> Exercises
);

public record ExerciseAssignmentRequest(
    Guid ExerciseId, 
    int Order, 
    int Sets, 
    string Reps, 
    int? RestTime, 
    string? Notes
);
