using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace ApelMusic.Entities
{
    public class Role : BaseEntity
    {
        [MaxLength(25)]
        public string? Name { get; set; }
    }
}