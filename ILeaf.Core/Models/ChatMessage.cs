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
    
    public partial class ChatMessage
    {
        public long Id { get; set; }
        public string MessageBody { get; set; }
        public byte MessageType { get; set; }
        public System.DateTime SendTime { get; set; }
        public long SenderId { get; set; }
        public Nullable<long> RecipientId { get; set; }
        public Nullable<long> GroupId { get; set; }
    
        public virtual Account Sender { get; set; }
        public virtual Account Recipient { get; set; }
        public virtual Group Group { get; set; }
    }
}