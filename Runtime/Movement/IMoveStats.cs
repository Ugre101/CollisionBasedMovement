namespace CollsionBasedMovement
{
    public interface IMoveStats
    {
        public int MaxJumpCount { get; }
        public float JumpStrength { get;  }
        public float SprintMultiplier { get;  }
        public float SwimSpeed { get;  }
        public float WalkSpeed { get;  }
    }
}