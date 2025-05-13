package com.example.prison_break;

import android.graphics.PointF;

import java.util.ArrayList;

public class VehiclesGenerate extends VehicleInfo {
    public static void vehicleGenerate(ArrayList<PointF> vehicles) {
        // vehicles
        for (int i = 0; i < 50; i++) {
            vehicles.add( new PointF(500, 500));
        }
        for (int i = 0; i < 50; i++) {
            vehicles.add( new PointF(100, 500));
        }
        for (int i = 0; i < 50; i++) {
            vehicles.add( new PointF(300, 500));
        }
    }


    public static void trucksGenerate(ArrayList<PointF> trucks) {
        // trucks
        for (int i = 0; i < 50; i++) {
            trucks.add( new PointF(100, 1200));
        }

        for (int i = 0; i < 50; i++) {
            trucks.add( new PointF(500, 1200));
        }
    }
    public static void tanksGenerate(ArrayList<PointF> tanks) {
        // tanks
        for (int i = 0; i < 50; i++) {
            tanks.add(new PointF(500, 300));
        }
        for (int i = 0; i < 50; i++) {
            tanks.add(new PointF(200, 300));
        }
    }
}
