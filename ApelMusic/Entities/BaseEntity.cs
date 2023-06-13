using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApelMusic.Entities
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? CreatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? Inactive { get; set; }
    }
}