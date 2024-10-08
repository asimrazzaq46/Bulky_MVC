using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.Utility;

public static class SD
{

    // USER ROLES
    public const string Role_Customer = "Customer";
    public const string Role_Company = "Company";
    public const string Role_Admin = "Admin";
    public const string Role_Employee = "Employee";

	// ORDER STATUS
	public const string Status_Pending = "Pending";
	public const string Status_Approved = "Approved";
	public const string Status_Processing = "Processing";
	public const string Status_Shipped = "Shipped";
	public const string Status_Cancelled = "Cancelled";
	public const string Status_Refunded = "Refunded";

	// PAYMENT STATUS
	public const string Payment_Status_Pending = "Pending";
	public const string Payment_Status_Approved = "Approved";
	public const string Payment_Status_DelayedPayment = "ApprovedForDelayedPayment";
	public const string Payment_Status_Rejected = "Rejected";


	// SESSION STRING
	public const string Session_Cart = "SessionShoppingCart";


}
