package com.example.prison_break.helpers;

public class GameConstants {

    private static String name;
    private static String player;
    private static String difficulty;




    public static String getPlayer() {
        return player;
    }
    public static void setPlayer(String p) {
        player = p;
    }

    public static String getName() {
        return name;
    }
    public static void setName(String n) {
        name = n;
    }

    public static void setDifficulty(String d) {
        difficulty = d;
    }
    public static String getDifficulty() {
        return difficulty;
    }

    public static final class Face_Dir {
        public static final int DOWN = 0;
        public static final int UP = 1;
        public static final int LEFT = 2;
        public static final int RIGHT = 3;
    }



}
