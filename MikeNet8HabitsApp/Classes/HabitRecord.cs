using SQLite;
using System;

namespace MikeNet8HabitsApp.Classes;

[Table("HabitRecords")]
public class HabitRecord
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateTime Date { get; set; }
    public bool IsCompleted { get; set; }
}
