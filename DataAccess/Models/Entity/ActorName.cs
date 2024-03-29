﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Models.Entity;

[PrimaryKey("Id", "Name")]
[Table("Actor_Names")]
public class ActorName
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Key]
    public string Name { get; set; } = null!;
}
