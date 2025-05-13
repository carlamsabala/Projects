package com.example.prison_break;

import android.graphics.PointF;
import android.graphics.RectF;

import com.example.prison_break.entities.GameCharacters;

import java.util.ArrayList;

public class VehicleInfo {

    //
    protected static ArrayList<PointF> vehicles = new ArrayList<>();
    protected static ArrayList<PointF> trucks = new ArrayList<>();

    protected static ArrayList<PointF> tanks = new ArrayList<>();

    protected static LogsInfo logs = new LogsInfo();
    PointF vehiclePos;
    protected static int VEHICLE_WIDTH = 130;
    protected static int VEHICLE_HEIGHT = 52;
    private int TRUCK_WIDTH = 360;
    private int TRUCK_HEIGHT = 270;
    private static int num = 0;


    // checks whether the cop car vehicles collide or not
    public boolean doVehiclesCollide(PointF pos1, PointF pos2) {
        // calculate the boxes for the two vehicles
        RectF box1 = new RectF(pos1.x - VEHICLE_WIDTH / 2, pos1.y - VEHICLE_HEIGHT / 2,
                pos1.x + VEHICLE_WIDTH / 2, pos1.y + VEHICLE_HEIGHT / 2);
        RectF box2 = new RectF(pos2.x - VEHICLE_WIDTH / 2, pos2.y - VEHICLE_HEIGHT / 2,
                pos2.x + VEHICLE_WIDTH / 2, pos2.y + VEHICLE_HEIGHT / 2);
        return box1.intersect(box2);
    }

    // update
    public static void update(double delta) {

        for(PointF pos: getVehicles()) {
            pos.x += delta * 800;

            if(pos.x >= 1920) {
                pos.x = 0;
            }
        }
        for(PointF pos: getTrucks()) {
            pos.x += delta * 500;

            if(pos.x >= 1920) {
                pos.x = 0;
            }
        }
        for(PointF pos: getTanks()) {
            pos.x += delta * 200;

            if(pos.x >= 1920) {
                pos.x = 0;
            }
        }
        for(PointF pos: logs.getLogs()) {
            pos.x += delta * 200;

            if(pos.x >= 1920) {
                pos.x = 0;
            }
        }
    }
    

    // checks collisions with trucks and decrements score
    public void checkCollisionWithTrucks() {
        RectF playerRect = new RectF(Player.getX(), Player.getY(),
                Player.getX() + Player.getPlayerSprite().getWidth(),
                Player.getY() + Player.getPlayerSprite().getHeight());

        for (PointF truck : trucks) {
            RectF vehicleRect = new RectF(truck.x, truck.y,
                    truck.x + TRUCK_WIDTH, truck.y + GameCharacters.TRUCK.getSprite().getHeight());

            if (playerRect.intersect(vehicleRect)) {
                // Collision detected, decrement score
                if (num > 0) {
                    GamePanel.setPoints(-((num / 2) + 1));
                }
                //GamePanel.setLives(gameLoop);
                Player.setX("reset");
                Player.setY("reset");
                // Remove the vehicle from the list
                trucks.remove(truck);
                break;
            }
        }
    }


    // getter method for vehicles
    public static ArrayList<PointF> getVehicles() {
        return vehicles;
    }

    // getter method for trucks
    public static ArrayList<PointF> getTrucks() {
        return trucks;
    }

    // getter method for tanks
    public static ArrayList<PointF> getTanks() {
        return tanks;
    }

}


