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
    
    public partial class Notification
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public System.DateTime SentTime { get; set; }
        public Nullable<long> SenderId { get; set; }
        public Nullable<long> RecipientId { get; set; }
        public Nullable<long> GroupId { get; set; }
        public string Section { get; set; }
        public byte Level { get; set; }
    }
}
