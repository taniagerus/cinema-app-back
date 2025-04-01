using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace cinema_app_back.Models
{
	[JsonConverter(typeof(JsonStringEnumConverter))]
	public enum Role
	{
		[EnumMember(Value = "Admin")]
		Admin,
		
		[EnumMember(Value = "User")]
		User,
        
        [EnumMember(Value = "Guest")]
        Guest
	}
}
