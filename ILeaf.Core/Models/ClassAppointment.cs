//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ILeaf.Core.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class ClassAppointment
    {
        public long Appointments_Id { get; set; }
        public long Classes_Id { get; set; }
        public string hf { get; set; }
    
        public virtual Appointment Appointment { get; set; }
        public virtual ClassInfo Class { get; set; }
    }
}
