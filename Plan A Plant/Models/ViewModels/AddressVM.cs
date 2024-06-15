namespace Plan_A_Plant.Models.ViewModels
{
    public class AddressVM
    {
        public string StreetAddress { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        public string FormattedAddress => $"{StreetAddress },{City},{State},{PostalCode}";   
    }
}
