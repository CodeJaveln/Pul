using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pul.PlayerBots
{
    class BasicPlayer : Player
    {
        public BasicPlayer()
            : base("Basic Player")
        {
        }

        public override int StickBidAmount()
        {
            return Hand.Count / 2;
        }

        public override Card CardToStack(List<Card> currentStack)
        {
            foreach (Card card in Hand)
            {
                if (PulFunctions.IsCardEligible(card, CurrentSuitCard.Suit, CurrentTrumf.Suit, Hand, out _))
                    return card;
            }

            throw new NotImplementedException();
        }
    }
}
