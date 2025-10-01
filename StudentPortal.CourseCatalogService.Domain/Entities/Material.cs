namespace StudentPortal.CourseCatalogService.Domain.Entities;
using System.Collections.Generic;
using System;
using StudentPortal.CourseCatalogService.Domain.Entities.Enums;
public class Material
{
    public int Id { get; set; }
    public int LessonId { get; set; }
    public string Title { get; set; }
    public string? Url { get; set; } 
    public MaterialType Type { get; set; }
    public int Order { get; set; }

    public Lesson? Lesson { get; set; }
}