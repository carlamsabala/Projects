package com.example.prison_break;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;

import android.graphics.PointF;
import android.graphics.RectF;

import org.junit.Before;
import org.junit.Test;

import java.util.ArrayList;

public class Sprint5Tests {

    private LogsInfo logsInfo;

    @Before
    public void setUp() {
        logsInfo = new LogsInfo();
    }

    // tests whether jumping on a log does not reset player position or score
    @Test
    public void testPlayerOnLog1() {
        // create player
        PlayerInfo p = new PlayerInfo();
        // create player and setting position
        PointF player1 = new PointF(110,840);
        p.setX(110);
        p.setY(840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // create log's second location
        logs.add(new PointF(100,850));

        // access score info
        ScoreInfo scoreInfo = new ScoreInfo();

        // set score
        scoreInfo.setPoints(43);

        // simulate a collision
        boolean collision = logsInfo.doLogsAndPlayerCollide(player1, logs.get(0));

        // if the player reaches the log, make sure that the points stay the same
        if (collision) {
            assertEquals(43,scoreInfo.getPoints());
        }
    }

    // tests whether player position changes when on top of the first log
    @Test
    public void TestPlayerMovesWithLog1() {
        PlayerInfo p = new PlayerInfo();
        // create player and setting position
        p.setX(110);
        p.setY(840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // create first log's location
        logs.add(new PointF(100,840));

        // simulate movement for the player on the log
        p.setNewX(45); // log1 moves 45 pixels per second

        // make sure player moves as fast as the log
        assertEquals(155,p.getX());
    }

    // tests whether jumping on the second log does not reset player position or score
    @Test
    public void TestPlayerOnLog2() {
        PlayerInfo p = new PlayerInfo();
        // create player and setting position
        p.setX(510);
        p.setY(840);
        PointF player1 = new PointF(510,840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // create first log's location
        logs.add(new PointF(500,850));

        // access score info
        ScoreInfo scoreInfo = new ScoreInfo();

        // set score
        scoreInfo.setPoints(123);

        // simulate a collision
        boolean collision = logsInfo.doLogsAndPlayerCollide(player1, logs.get(0));

        // if the player reaches the log, make sure that the points stay the same
        if (collision) {
            assertEquals(123,scoreInfo.getPoints());
        }
    }

    // tests whether player position changes when on top of the second log
    @Test
    public void TestPlayerMovesWithLog2() {
        PlayerInfo p = new PlayerInfo();
        // create player and setting position
        p.setX(500);
        p.setY(840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // create first log's location
        logs.add(new PointF(500,840));

        // simulate movement for the player on the log
        p.setNewX(11); // log1 moves 45 pixels per second

        // make sure player moves as fast as the log
        assertEquals(511,p.getX());
    }

    // tests when player reaches a goal tile, they get the end message
    @Test
    public void testEndMessage() {
        PlayerInfo p = new PlayerInfo();
        // create player and setting position
        p.setX(500);
        p.setY(50); // specifically where goal tiles are
        // make sure message is sent when player reaches the goal tile
        assertEquals("Congratulations!",p.endMessage(p.getY()));
    }

    @Test
    public void testEndMessageNotGoalTile() {
        PlayerInfo p = new PlayerInfo();
        // create player and setting position
        p.setX(500);
        p.setY(240);            // specifically where goal tiles are not located
        // make sure message isn't sent if player is not on a goal tile
        assertEquals("",p.endMessage(p.getY()));
    }

    // tests that at the end tile, player has most amount of points
    @Test
    public void testGreatestPoints() {
        PlayerInfo p = new PlayerInfo();
        ScoreInfo s = new ScoreInfo();
        s.setPoints(0);                     // points start off at 0
        // create player at starting position
        p.setX(500);
        p.setY(1500);
        int num = -100;
        int curr = 1500;
        // add player in different parts of the map - takes 13 to reach goal tile
        while (p.getY() >= 100) {
            p.setY(curr+num);
            s.calculateScore();
            curr += num;
        }
        s.setPoints(140);
        assertEquals(165, s.getPoints());

    }
    // tests message when player runs out of lives
    @Test
    public void testNoLifeMessage() {
        ScoreInfo s = new ScoreInfo();
        s.numLives = 0;
        assertEquals("Better luck next time!",s.endMessage());
    }

    // makes sure the no life message doesn't show up when player has lives
    @Test
    public void testNoLifeMessageWithLives() {
        ScoreInfo s = new ScoreInfo();
        s.numLives = 1;
        assertEquals("",s.endMessage());
    }

    // check the predetermined equal log speeds
    @Test
    public void testLogSpeeds() {
        Logs2Info l2 = new Logs2Info();
        // speed of log 1 vs log 2
        assertEquals(l2.log2Speed,logsInfo.logSpeed);
    }

    // test restart
    @Test
    public void testRestart() {
        ScoreInfo s = new ScoreInfo();
        PlayerInfo p = new PlayerInfo();
        p.resetPosition();
        s.numLives = 0;
        assertEquals(0, s.numLives);        // check reset lives
        assertEquals(0, s.getPoints());     // check reset points
        assertEquals(500, p.getX());        // check start X
        assertEquals(1500, p.getY());

    }

    // check that the logs don't collide with each other
    @Test
    public void checkLogsDontCollide() {
        Logs2Info l2 = new Logs2Info();
        // calculate the boxes for the second log
        RectF box1 = new RectF(100 - l2.LOG_WIDTH / 2, 700 - l2.LOG_HEIGHT / 2,
                100 + l2.LOG_WIDTH / 2, 700 + l2.LOG_HEIGHT / 2);
        // calculate the boxes for the first log
        RectF box2 = new RectF(500 - logsInfo.LOG_WIDTH / 2, 850 - logsInfo.LOG_HEIGHT / 2,
                500 + logsInfo.LOG_WIDTH / 2, 850 + logsInfo.LOG_HEIGHT / 2);
        assertFalse(box1.intersect(box2));
    }

}
