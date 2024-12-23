﻿using Bulky.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAcess.Repositery.IRepositery;

public interface IOrderHeaderRepositery:IRepositery<OrderHeader>
{
    void Update(OrderHeader obj);
     void UpdateStatus(int id,string OrderStatus,string? paymentStatus = null);
     void UpdateStripePaymentID(int id,string sessionID, string paymentIntentId);


}
