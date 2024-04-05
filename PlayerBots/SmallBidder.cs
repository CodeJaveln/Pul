using System.Collections.Generic;

namespace Pul
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
                if (PulFunctions.IsCardEligible(card, CurrentSuitCard.Suit, CurrentTrumf.Suit, Hand, out _))
                {
                    return card;
                }
            }

            return Hand[0];
        }
    }
}
