namespace ZombieDiner.Core
{
    public enum GameStage
    {
        Stage1_Normal,  // Stage 1: The normal restaurant and human customers
        Cutscene,       // The Storyboard transition screen
        Stage2_Zombie,  // Stage 2: The zombie restaurant and terrifying orders
        GameOver        // Game over and session end
    }
}