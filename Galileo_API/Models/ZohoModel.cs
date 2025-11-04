namespace PgxAPI_Externo.Models.NewFolder
{
    public class Source
    {
        public string? AppName { get; set; }
        public string? ExtId { get; set; }
        public string? Permalink { get; set; }
        public string? Type { get; set; }
        public string? AppPhotoURL { get; set; }
    }

    public class LayoutDetails
    {
        public string? Id { get; set; }
        public string? LayoutName { get; set; }
    }

    public class Contact
    {
        public string? Id { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
        public string? PhotoURL { get; set; }
    }

    public class DescriptionDetails
    {
        public string? ContentType { get; set; }
        public string? Description { get; set; }
    }

    public class ContactDetails
    {
        public string? Id { get; set; }
        public string? LastName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
        public string? PhotoURL { get; set; }
    }

    public class CustomFields
    {
        public object? CustoFields { get; set; }
    }

    public class Ticket
    {
        public string? ModifiedTime { get; set; }
        public string? SubCategory { get; set; }
        public string? StatusType { get; set; }
        public string? Subject { get; set; }
        public string? DueDate { get; set; }
        public string? DepartmentId { get; set; }
        public string? Channel { get; set; }
        public string? OnHoldTime { get; set; }
        public string? Language { get; set; }
        public Source? Source { get; set; }
        public string? Resolution { get; set; }
        public List<string>? SharedDepartments { get; set; }
        public string? ClosedTime { get; set; }
        public string? ApprovalCount { get; set; }
        public bool? IsOverDue { get; set; }
        public bool? IsTrashed { get; set; }
        public string? CreatedTime { get; set; }
        public string? Id { get; set; }
        public bool? IsResponseOverdue { get; set; }
        public string? CustomerResponseTime { get; set; }
        public string? ContactId { get; set; }
        public string? ThreadCount { get; set; }
        public List<string>? SecondaryContacts { get; set; }
        public string? Priority { get; set; }
        public string? Classification { get; set; }
        public string? CommentCount { get; set; }
        public string? TaskCount { get; set; }
        public string? AccountId { get; set; }
        public string? Phone { get; set; }
        public string? WebUrl { get; set; }
        public bool? IsSpam { get; set; }
        public string? Status { get; set; }
        public List<string>? EntitySkills { get; set; }
        public string? TicketNumber { get; set; }
        public object? CustomFields { get; set; }
        public string? Sentiment { get; set; }
        public bool? IsArchived { get; set; }
        public string? Description { get; set; }
        public string? TimeEntryCount { get; set; }
        public string? ChannelRelatedInfo { get; set; }
        public string? ResponseDueDate { get; set; }
        public bool? IsDeleted { get; set; }
        public string? ModifiedBy { get; set; }
        public string? FollowerCount { get; set; }
        public string? Email { get; set; }
        public LayoutDetails? LayoutDetails { get; set; }
        public string? ChannelRelatedData { get; set; }
        public Contact? Contact { get; set; }
        public string? TeamId { get; set; }
        public string? GroupId { get; set; }
        public DescriptionDetails? DescriptionDetails { get; set; }
        public string? ProductId { get; set; }
        public string? TeamName { get; set; }
        public string? ApprovalCountNum { get; set; }
        public string? ThreadCountNum { get; set; }
        public string? CommentCountNum { get; set; }
        public string? TaskCountNum { get; set; }
        public string? TimeEntryCountNum { get; set; }
        public string? FollowerCountNum { get; set; }
        public ContactDetails? ContactDetails { get; set; }
        public object? cf { get; set; }
    }

    public class DataModel
    {
        public List<Ticket>? data { get; set; }
        public int count { get; set; }
    }

    public class ZohoModel
    {
        public string access_token { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public string scope { get; set; } = string.Empty;
        public string api_domain { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public string expires_in { get; set; } = string.Empty;
    }

    public class ZohoAuthModel
    {
        public string access_token { get; set; } = "1000.ede7664dfbdb31a30044ce69a1a31309.147a04fcf4d5f906a6881d878ee87b11";
        public string scope { get; set; } = "Desk.tickets.ALL Desk.search.READ";
        public string api_domain { get; set; } = "https://www.zohoapis.com";
        public string token_type { get; set; } = "Bearer";
        public int expires_in { get; set; } = 3600;
    }

    public class ZohoAuthTokenModel
    {
        public string refresh_token { get; set; } = "1000.ede7664dfbdb31a30044ce69a1a31309.147a04fcf4d5f906a6881d878ee87b11";
        public string client_id { get; set; } = "1000.IN873D3QJRFVKWIZQ1UQTEEWNTCVGU";
        public string client_secret { get; set; } = "79ae19f21a6e50dc7f348fc42530ae0d1ffce5c169";
        public string grant_type { get; set; } = "refresh_token";
    }

    public class ZohoParametros
    {
        public string code { get; set; } = "1000.7e0b2a8025b96cab4109cab004bf2f96.6db2679aa2df780fc50d1531b1b347d1";
        public string client_id { get; set; } = "1000.IN873D3QJRFVKWIZQ1UQTEEWNTCVGU";
        public string client_secret { get; set; } = "79ae19f21a6e50dc7f348fc42530ae0d1ffce5c169";
        public string redirect_uri { get; set; } = "https://www.google.com/";
        public string grant_type { get; set; } = "authorization_code";
        public string scope { get; set; } = "Desk.tickets.ALL Desk.search.READ";
    }

    public class TokenModel
    {
        public string? int_id { get; set; }
        public string? token { get; set; }
        public Nullable<DateTime> fecha { get; set; }
    }

    public class TicketAttachments 
    { 
    
        public List<AttachmentsLista>? attachments { get; set; }
        public int attachmentCount { get; set; }

    }

    public class AttachmentsLista
    {
        public string id { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public string size { get; set; } = string.Empty;
        public string href { get; set; } = string.Empty;
        public string previewurl { get; set; } = string.Empty;
    }

    public class AttachmentBase
    {
        public List<AttachmentBaseList>? data { get; set; }
    }

    public class AttachmentBaseList
    {
        public string? id { get; set; }
        public int attachmentCount { get; set; }
    }
}
