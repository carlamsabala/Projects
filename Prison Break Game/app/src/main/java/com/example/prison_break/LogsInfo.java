package com.example.prison_break;

import android.graphics.PointF;
import android.graphics.RectF;

import java.util.ArrayList;

public class LogsInfo extends ScoreInfo {
    protected static ArrayList<PointF> logs = new ArrayList<>();
    protected static PlayerInfo player = new PlayerInfo();
    private static ScoreInfo scoreInfo = new ScoreInfo();
    protected static final int LOG_WIDTH = 435;
    protected static final int LOG_HEIGHT = 130;
    protected static int logSpeed = 350;
    private static int num = 0;



    // check whether player collides with logs
    public static boolean checkCollisionWithLogs() {
        RectF playerRect = new RectF(Player.getX(), Player.getY(),
                Player.getX() + Player.getPlayerSprite().getWidth(),
                Player.getY() + Player.getPlayerSprite().getHeight());
        for (PointF log : logs) {
            RectF vehicleRect = new RectF(log.x, log.y,
                    log.x + LOG_WIDTH, log.y + LOG_HEIGHT);

            if (playerRect.intersect(vehicleRect)) {
                return true;
            }
        }
        return false;
    }

    // method for unit testing
    public boolean doLogsAndPlayerCollide(PointF pos1, PointF pos2) {
        // calculate the boxes for the player
        RectF box1 = new RectF(pos1.x - 112 / 2, pos1.y - 112 / 2,
                pos1.x + 112 / 2, pos1.y + 112 / 2);
        // calculate the boxes for the log
        RectF box2 = new RectF(pos2.x - LOG_WIDTH / 2, pos2.y - LOG_HEIGHT / 2,
                pos2.x + LOG_WIDTH / 2, pos2.y + LOG_HEIGHT / 2);
        return box1.intersect(box2);
    }


    // log generation
    public static void logsGenerate(ArrayList<PointF> logs) {
        //logs
        for (int i = 0; i < 50; i++) {
            logs.add(new PointF(500, 850));
        }
    }

    public void setLives(GameLoop gameLoop) {
        if (numLives > 1) {
            numLives -= 1;
        } else if (numLives == 1) {
            gameLoop.stopGameLoop();
        }
    }
    //getter method for logs
    public static ArrayList<PointF> getLogs() { return logs; }
    public static int getLogWidth() {return LOG_WIDTH;}
    public static int getLogHeight() {return LOG_HEIGHT;}


}
