using rapid.core.app.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace rapid.core.app.Source
{
    public class RapidDBContext(DbContextOptions<RapidDBContext> options) : DbContext(options)
    {
    }
}
