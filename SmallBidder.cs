using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewPul
{
    class SmallBidder : Player
    {
        public SmallBidder()
            : base("SmallBidder")
        {
            
        }

        public override int StickBidAmount()
        {
            return 0;
        }

        public override Card CardToStack(List<Card> currentStack)
        {
            foreach (Card card in Hand)
            {
                if (PulRevised.IsCardEligible(card, CurrentSuitCard.Suit, CurrentTrumf.Suit, Hand, out PulRevised.IlelegibleReason ilelegibleReasons))
                {
                    return card;
                }
            }

            return Hand[0];
        }
    }
}
