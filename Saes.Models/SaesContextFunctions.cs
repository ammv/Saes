using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Saes.Models.Extensions;
using Saes.Models.Schemas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Saes.Models
{
    public partial class SaesContext
    {
        
        void OnModelCreatingPartial(ModelBuilder modelBuilder)
        {
            
        }
    }
}
