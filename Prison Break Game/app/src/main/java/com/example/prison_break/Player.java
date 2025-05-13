package com.example.prison_break;

import android.graphics.Bitmap;

import com.example.prison_break.entities.GameCharacters;
import com.example.prison_break.helpers.GameConstants;


public class Player {
    private static int x;
    private static int y;
    private static String name;
    private static Bitmap player;
    private static int changeX;
    private static int changeY;

    public static final int START_X = 500;
    public static final int START_Y = 1500;

    public Player() {
        setPlayer();
        x = 500 + changeX;
        y = 1500 + changeY;

    }
    public static void setY(String s) {
        if (s == "up" && y > 0) {
            changeY -= 50;
        } else if (s == "down" && y <= 1500) {
            changeY += 50;
        } else if (s == "reset") {
            changeY = 0;
        }
    }
    public static void setX(String s) {
            if (s == "left" && x > 0) {
                changeX -= 50;
            } else if (s == "right" && x < 1000) {
                changeX += 50;
            } else if (s == "reset") {
                changeX = 0;
            } else if (s == "log2") {
                changeX += 11;
            }else {
                changeX += 45;
            }
    }

    public static void setChangeX(float num) {
        changeX += num;
    }


    public static int getX() {
        return x;
    }
    public static int getY() {
        return y;
    }
    private void setPlayer() {
        if (GameConstants.getPlayer() == "dude1") {
            player = GameCharacters.PLAYER1.getSprite();
        } else if (GameConstants.getPlayer() == "dude2") {
            player = GameCharacters.PLAYER2.getSprite();
        } else {
            player = GameCharacters.PLAYER3.getSprite();
        }

    }

    public static Bitmap getPlayerSprite() {
        return player;
    }

    public void resetPosition() {
        x = 500;
        y = 1500;
    }



}

