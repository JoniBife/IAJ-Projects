namespace Assets.Scripts.IAJ.Unity.DecisionMaking.MCTS
{
    public class Reward
    {
        public float Value { get; set; }
        public int PlayerID { get; set; }

        public float GetRewardForNode(MCTSNode node)
        {
            if (node.Parent == null)
                return this.Value;

            if (node.Parent.PlayerID == PlayerID)
                return this.Value;
            else
                return 1 - this.Value;
        }

    }
}
