package com.example.prison_break;

public class PlayerInfo {
    // variables
    private static int x;
    private static int y;
    private static final int PLAYER1_HEIGHT = 112;
    private static final int PLAYER1_WIDTH = 112;
    private static final int PLAYER2_HEIGHT = 112;
    private static final int PLAYER2_WIDTH = 112;
    private static final int PLAYER3_HEIGHT = 112;
    private static final int PLAYER3_WIDTH = 112;
    private static int changeX;
    private static ScoreInfo scoreInfo = new ScoreInfo();
    private static int numLives;

    public void resetPosition() {
        x = 500;
        y = 1500;
        scoreInfo.resetPoints();
    }
    public static int getPlayer1Height() {return PLAYER1_HEIGHT;}
    public static int getPlayer1Width() {return PLAYER1_WIDTH;}
    public static int getPlayer2Height() {return PLAYER2_HEIGHT;}
    public static int getPlayer2Width() {return PLAYER2_WIDTH;};
    public static int getPlayer3Height() {return PLAYER3_HEIGHT;}
    public static int getPlayer3Width() {return PLAYER3_WIDTH;}

    public static int getX() {return x;}
    public static int getY() {return y;}
    public static void setX(int change) {x=change;}
    public static void setY(int change) {y=change;}
    // used for testing purposes - don't delete! (to separate UI from business logic)
    public static void setNewX(int changeX) {x+=changeX;}
    public static void setNewY(int changeY) {y-=changeY;}
    // used for testing purposes - don't delete (to separate UI from business logic)
    protected static String endMessage(int y) {if (y < 100) {return "Congratulations!";} return "";}
}
