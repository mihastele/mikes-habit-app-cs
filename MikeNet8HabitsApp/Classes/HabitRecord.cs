using SQLite;
using System;

namespace MikeNet8HabitsApp.Classes
{
    [Table("HabitRecord")]
    public class HabitRecord
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        
        [Indexed]
        public int HabitId { get; set; }
        
        [Indexed]
        public DateTime Date { get; set; }
        
        public bool IsCompleted { get; set; }
    }
}