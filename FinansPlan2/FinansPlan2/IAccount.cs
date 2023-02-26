using System;
using System.Collections.Generic;

namespace FinansPlan2
{
    public interface IAccount
    {
        // AlfaCreditCardState CurrentState { get; set; }
        CardTicker Ticker { get; }
        Bank Bank { get; }

        List<Error> OnRashod(RashodRequest request);
        CanRashodResponse CanRashod(RashodRequest request);

        void OnPrihod(RashodRequest request);
        CanRashodResponse CanPrihod(RashodRequest request);
    }
}