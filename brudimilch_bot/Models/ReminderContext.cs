using brudimilch_bot;
using brudimilch_bot.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

public class ReminderContext : DbContext
{
    public DbSet<Reminder> Reminders { get; set; }

    public string DbPath { get; }

    public ReminderContext()
    {
        //var folder = Environment.SpecialFolder.LocalApplicationData;
        //var path = Environment.GetFolderPath(folder);
        //DbPath = System.IO.Path.Join(path, "reminders.db");
        DbPath = AppSettings.DbPath;
    }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={AppSettings.DbPath}");
}