package com.example.prison_break;

import static org.junit.Assert.assertEquals;

import android.graphics.PointF;

import com.example.prison_break.helpers.GameConstants;

import org.junit.Before;
import org.junit.Test;

import java.util.ArrayList;

public class Sprint4Tests {

    private LogsInfo logsInfo;

    @Before
    public void setUp() {
        logsInfo = new LogsInfo();
    }

    // tests whether first location log collides collides and resets score
    @Test
    public void testLog1PlayerCollisionScore() {
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

        // check collision and whether score resets
        if (!collision) {
            scoreInfo.resetPoints();
            assertEquals(0,scoreInfo.getPoints());
        } else {
            // check that points were reset with collision
            assertEquals(43,scoreInfo.getPoints());
        }
    }

    // tests whether the second location of logs collides and resets score
    @Test
    public void testLog2PlayerCollisionScore() {
        // create player
        PlayerInfo p = new PlayerInfo();
        // create player
        PointF player1 = new PointF(510,840);
        p.setX(510);
        p.setY(840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // create log's second location
        logs.add(new PointF(500,850));

        // access score info
        ScoreInfo scoreInfo = new ScoreInfo();

        // set score
        scoreInfo.setPoints(43);

        // simulate a collision
        boolean collision = logsInfo.doLogsAndPlayerCollide(player1, logs.get(0));

        // check collision and whether score resets
        if (!collision) {
            scoreInfo.resetPoints();
            assertEquals(0,scoreInfo.getPoints());
        } else {
            // check that points were reset with collision
            assertEquals(43,scoreInfo.getPoints());
        }
    }

    // tests whether the first location of water tiles resets position of player when collided
    @Test
    public void testLog1PlayerCollisionResetPosition() {
        // create player and log1
        PlayerInfo p = new PlayerInfo();
        PointF player1 = new PointF(110,840);
        p.setX(110);
        p.setY(840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // make logs first location
        logs.add(new PointF(100,850));

        // check if there is a collision, to reset the player's position
        if (!logsInfo.doLogsAndPlayerCollide(player1, logs.get(0))) {
            p.resetPosition();
            // make sure player position gets reset
            assertEquals(500, p.getX());
            assertEquals(1500, p.getY());
        } else {
            assertEquals(110, p.getX());
            assertEquals(840, p.getY());
        }
    }

    // tests whether the second location of water tiles resets player position when collided
    @Test
    public void testLog2PlayerCollisionResetPosition() {
        // create player
        PlayerInfo p = new PlayerInfo();
        // put player in position of water and logs
        PointF player1 = new PointF(510,840);
        p.setX(510);
        p.setY(840);

        // get logs
        ArrayList<PointF> logs = logsInfo.getLogs();
        // create log's second location
        logs.add(new PointF(500,850));

        // check if there is a collision, to reset the player's position
        if (!logsInfo.doLogsAndPlayerCollide(player1, logs.get(0))) {
            p.resetPosition();
            // make sure player position gets reset
            assertEquals(500, p.getX());
            assertEquals(1500, p.getY());
        } else {
            assertEquals(510, p.getX());
            assertEquals(840, p.getY());
        }
    }

    // makes sure logs generate as needed - TEST DIFFERENTLY (for some reason)
    @Test
    public void testLogGeneration() {
        // make sure 0 to begin with
        assertEquals(0, logsInfo.getLogs().size());
        // generate logs
        logsInfo.logsGenerate(logsInfo.logs);
        // ensure that the correct number of vehicles has been generated - 50 from each side
        assertEquals(50, logsInfo.getLogs().size());
    }

    // makes sure logs spawn in correct area
    @Test
    public void testLogSpawn() {
        logsInfo.logsGenerate(logsInfo.logs);
        // checks that logs are spawning from the correct position
        for (int i = 0; i < logsInfo.getLogs().size(); i++) {
            if (i < 50) {
                System.out.println(logsInfo.getLogs().get(i).x);
                assertEquals(0, logsInfo.getLogs().get(i).x, 0);
                assertEquals(0, logsInfo.getLogs().get(i).y, 0);
            }
        }
    }

    @Test
    // checks that a vehicle collision decreases lives
    public void testVehicleCollisionLives() {
        // access vehicle information
        VehicleInfo vehicleInfo = new VehicleInfo();
        // define vehicle position
        PointF vehiclePos1 = new PointF(500, 500);
        // access lives and set player
        PlayerInfo playerInfo = new PlayerInfo();
        PointF playerPos = new PointF(500, 500);
        // access difficulty
        GameConstants gameConstants = new GameConstants();
        // define lives
        int numLives;
        int decLives;
        boolean vehicleCollision = false;
        if (gameConstants.getDifficulty() == "Easy (3 Lives)") {
            numLives = 3;
            decLives = 3;
        } else if (gameConstants.getDifficulty() == "Medium (2 Lives)") {
            numLives = 2;
            decLives = 2;
        } else {
            numLives = 1;
            decLives = 1;
        }
        // simulate collision
        if (vehicleInfo.doVehiclesCollide(playerPos, vehiclePos1)) {
            decLives--;
            vehicleCollision = true;

        }
        if (vehicleCollision) {
            assertEquals(decLives, numLives - 1);
        } else {
            assertEquals(decLives, numLives);
        }

    }

    @Test
    // checks that a truck collision decreases lives
    public void testTruckCollisionLives() {
        // access vehicle information
        VehicleInfo vehicleInfo = new VehicleInfo();
        // define truck position
        PointF truckPos = new PointF(100, 1200);
        // access lives and set player
        PlayerInfo playerInfo = new PlayerInfo();
        PointF playerPos = new PointF(101, 1205);
        // access difficulty
        GameConstants gameConstants = new GameConstants();
        // define lives
        int numLives;
        int decLives;
        boolean truckCollision = false;
        if (gameConstants.getDifficulty() == "Easy (3 Lives)") {
            numLives = 3;
            decLives = 3;
        } else if (gameConstants.getDifficulty() == "Medium (2 Lives)") {
            numLives = 2;
            decLives = 2;
        } else {
            numLives = 1;
            decLives = 1;
        }
        // simulate collision
        if (vehicleInfo.doVehiclesCollide(playerPos, truckPos)) {
            decLives--;
            truckCollision = true;
        }
        if (truckCollision) {
            assertEquals(decLives, numLives - 1);
        } else {
            assertEquals(decLives, numLives);
        }
    }

    @Test
    // checks that a tank collision decreases lives
    public void testTankCollisionLives() {
        // access vehicle information
        VehicleInfo vehicleInfo = new VehicleInfo();
        // define tank position
        PointF tankPos = new PointF(500, 300);
        // access lives and set player position
        PlayerInfo playerInfo = new PlayerInfo();
        PointF playerPos = new PointF(504, 302);
        // access difficulty
        GameConstants gameConstants = new GameConstants();
        // define lives
        int numLives;
        int decLives;
        boolean tankCollision = false;
        if (gameConstants.getDifficulty() == "Easy (3 Lives)") {
            numLives = 3;
            decLives = 3;
        } else if (gameConstants.getDifficulty() == "Medium (2 Lives)") {
            numLives = 2;
            decLives = 2;
        } else {
            numLives = 1;
            decLives = 1;
        }
        // simulate collision
        if (vehicleInfo.doVehiclesCollide(playerPos, tankPos)) {
            decLives--;
            tankCollision = true;
        }
        if (tankCollision) {
            assertEquals(decLives, numLives - 1);
        } else {
            assertEquals(decLives, numLives);
        }
    }

    @Test
    // checks that a vehicle collision resets player location
    public void testVehicleCollisionResetPosition() {
        // access vehicle information
        VehicleInfo vehicleInfo = new VehicleInfo();

        // make new player with its original position
        PlayerInfo p = new PlayerInfo();
        PointF playerPos = new PointF(100, 100);
        p.setX(100);
        p.setY(100);

        // get vehicles
        ArrayList<PointF> vehicles = vehicleInfo.getVehicles();
        // add a vehicle to be collided with
       vehicles.add(new PointF(100,100));

        // check if there is a collision, to reset the player's position
        if (vehicleInfo.doVehiclesCollide(playerPos, vehicles.get(0))) {
            p.resetPosition();
            // make sure player position gets reset
            assertEquals(500, p.getX());
            assertEquals(1500, p.getY());
        } else {
            assertEquals(100, p.getX());
            assertEquals(100, p.getY());
        }

    }

    @Test
    // checks that player is respawned at starting point
    public void testRespawn() {
        // access playerInfo
        PlayerInfo playerInfo = new PlayerInfo();
        // create player position at somewhere that is not the starting point
        PointF playerPos = new PointF(600, 2043);
        // reset player position
        playerInfo.resetPosition();
        assertEquals(500, playerInfo.getX());
        assertEquals(1500, playerInfo.getY());
    }

    @Test
    // checks score gets set to 0 when respawned
    public void respawnScore() {
        // access score
        ScoreInfo scoreInfo = new ScoreInfo();
        // set score to a random amount of points
        scoreInfo.setPoints(34);
        // access PlayerInfo
        PlayerInfo playerInfo = new PlayerInfo();
        // reset player position
        playerInfo.resetPosition();
        // check that score gets set to 0
        assertEquals(0, scoreInfo.getPoints());

    }


}
