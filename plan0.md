# MikeNet8HabitsApp Plan

## Notes

- The user wants to fix three issues in the `MikeNet8HabitsApp`.
- The app uses .NET 8 and MAUI.
- The `sqlite-net-pcl` NuGet package has been added for database operations.
- The date formatting logic for the current day in `MainPage.xaml.cs` has been fixed.
- The `IsVisible` bindings in `MainPage.xaml` have been updated. The `NullToBooleanConverter` is now unused and can be removed from the page resources.
- A `DatabaseService` has been created to handle SQLite operations.
- New pages `AddHabitPage.xaml` and its code-behind `AddHabitPage.xaml.cs` need to be fully implemented.
- The `NullToBooleanConverter` has been removed from `MainPage.xaml`.
- `DatabaseService` now correctly handles `Habit` and `CountableHabit` types for saving and retrieving.
- `AddHabitPage.xaml.cs` is implemented.

## Task List

- [x] **Ticket 1: Fix '+' button visibility on MainPage**
    - [x] Investigate `MainPage.xaml` to understand how habits are displayed.
    - [x] Identify the logic that shows the `+` button.
    - [x] Add `IsCountable` virtual property to `Habit` and override in `CountableHabit`.
    - [x] Modify the `IsVisible` binding for the `+` button and related labels in `MainPage.xaml` to use the `IsCountable` property.
    - [x] Remove unused `NullToBooleanConverter` from `MainPage.xaml`.
- [x] **Ticket 3: Fix date formatting for the current day**
    - [x] Investigate the code responsible for date formatting in `MainPage.xaml.cs`.
    - [x] Identify that the date formatting logic in `UpdateDateDisplay` is incorrect.
    - [x] Fix the date formatting logic in `UpdateDateDisplay` method in `MainPage.xaml.cs`.
- [x] **Ticket 2: Implement "Add Habit" page and database**
    - [x] Add `sqlite-net-pcl` NuGet package and remove the `SQLite` package.
    - [x] Update `DatabaseService` class to handle SQLite operations (initialization, getting all habits, saving specific habit types).
    - [x] Register `DatabaseService` as a singleton in `MauiProgram.cs`.
    - [x] Create a new page `AddHabitPage.xaml` for adding habits.
    - [x] Add UI elements for habit details (name, description, type (normal/countable), target count).
    - [x] Create and implement `AddHabitPage.xaml.cs` to save the new habit to the SQLite database.
    - [x] Update `MainPage.xaml.cs` `OnAddHabitClicked` to navigate to `AddHabitPage`.
    - [x] Refactor `MainPage.xaml.cs` to load habits from the database instead of using sample data.
        - [x] Inject `DatabaseService` into `MainPage.xaml.cs`.
        - [x] Modify `LoadHabitsForDate` to fetch habits from `DatabaseService.GetAllHabitsAsync()` (and filter by date if necessary, or adjust DB service).
        - [x] Update `OnIncrementCountClicked` and `OnHabitCheckedChanged` to save changes to the database.
        - [x] Ensure `MainPage` refreshes when habits are added/updated (e.g., in `OnAppearing`).

## Current Goal

All tickets completed. Test the application.