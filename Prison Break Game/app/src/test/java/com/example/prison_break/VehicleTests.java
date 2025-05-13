package com.example.prison_break;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertFalse;

import android.graphics.PointF;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.JUnit4;

import java.util.ArrayList;

// SPRINT #3 UNIT TESTS (6 vehicle related unit tests)


@RunWith(JUnit4.class)
//@Config(manifest= Config.NONE)
public class VehicleTests {
    // local variables
    private VehicleInfo vehicleInfo;
    private VehiclesGenerate vehicleGenerate;

    @Before
    public void setUp() {
        VehicleInfo vehicleInfo = new VehicleInfo();
        vehicleGenerate = new VehiclesGenerate();
    }

////////////////////////////////////////////////////////////////

    // tests whether the vehicles are generated correctly
    @Test
    public void testVehicleGeneration() {
        // assert there are no vehicles to begin with
        assertEquals(0, vehicleInfo.getVehicles().size());
        // generate vehicles
        vehicleGenerate.vehicleGenerate(vehicleInfo.vehicles);
        // ensure that the correct number of vehicles has been generated - 50 from each side
        assertEquals(150, vehicleInfo.getVehicles().size());
    }

    @Test
    public void testTruckGeneration() {
        // assert there are no trucks to begin with
        assertEquals(0,vehicleInfo.getTrucks().size());
        // generate trucks
        vehicleGenerate.trucksGenerate(vehicleInfo.trucks);
        // ensure that the correct number of vehicles has been generated - 50 from each side
        assertEquals(100, vehicleInfo.getTrucks().size());
    }

    @Test
    public void testTankGeneration() {
        // assert there are no trucks to begin with
        assertEquals(0,vehicleInfo.getTanks().size());
        // generate trucks
        vehicleGenerate.tanksGenerate(vehicleInfo.tanks);
        // ensure that the correct number of vehicles has been generated - 50 from each side
        assertEquals(100, vehicleInfo.getTanks().size());
    }

    // tests whether vehicles collide to each other
    @Test
    public void testVehicleCollision() {
        VehicleInfo vehicleInfo = new VehicleInfo();
        // create two vehicles
        PointF vehicle1 = new PointF(100,100);
        PointF vehicle2 = new PointF(120,100);

        // simulate a collision
        boolean collision = vehicleInfo.doVehiclesCollide(vehicle1, vehicle2);

        // assert if collision occurred
        assertFalse(collision);
    }


    // tests whether vehicle spawn from different sides of the road
    @Test
    public void testVehicleSpawn() {
        // run the code to spawn the vehicles
        vehicleGenerate.vehicleGenerate(vehicleInfo.vehicles);
        // checks that vehicles are spawning from the correct position
        for (int i = 0; i < vehicleInfo.getVehicles().size(); i++) {
            if (i < 50) {
            System.out.println(vehicleInfo.getVehicles().get(i).x);
            assertEquals(0, vehicleInfo.getVehicles().get(i).x, 0);
            assertEquals(0, vehicleInfo.getVehicles().get(i).y, 0);
            }
        }
    }

    // tests whether trucks spawn from different sides of the road
    @Test
    public void testTruckSpawn() {
        // run the code to spawn the vehicles
        vehicleGenerate.trucksGenerate(vehicleInfo.trucks);
        // checks that vehicles are spawning from the correct position
        for (int i = 0; i < vehicleInfo.getTrucks().size(); i++) {
            if (i < 50) {
                System.out.println(vehicleInfo.getTrucks().get(i).x);
                assertEquals(0, vehicleInfo.getTrucks().get(i).x, 0);
                assertEquals(0, vehicleInfo.getTrucks().get(i).y, 0);
            }
        }
    }

    // tests whether tanks spawn from different sides of the road
    @Test
    public void testTankSpawn() {
        // run the code to spawn the vehicles
        vehicleGenerate.tanksGenerate(vehicleInfo.tanks);
        // checks that vehicles are spawning from the correct position
        for (int i = 0; i < vehicleInfo.getTanks().size(); i++) {
            if (i < 50) {
                System.out.println(vehicleInfo.getTanks().get(i).x);
                assertEquals(0, vehicleInfo.getTanks().get(i).x, 0);
                assertEquals(0, vehicleInfo.getTanks().get(i).y, 0);
            }
        }
    }

    //tests if vehicles have speed differences and whether it turns out correctly
    @Test
    public void testSpeedDifference() {
        // add vehicles with their different speeds to the game panel
        ArrayList<PointF> vehicles = vehicleInfo.getVehicles();
        ArrayList<PointF> trucks = vehicleInfo.getTrucks();
        ArrayList<PointF> tanks = vehicleInfo.getTanks();

        // add a vehicle with speed 800
        vehicles.add(new PointF(0, 0));

        // add a truck with speed 500
        trucks.add(new PointF(0, 0));

        // add a tank with speed 200
        tanks.add(new PointF(0, 0));

        // fixed double delta
        double delta = 0.5;

        // call update method
        vehicleInfo.update(delta);

        // The vehicle should have moved by 400 units (800 * 0.5)
        assertEquals(400, vehicles.get(0).x, 0.001);

        // The truck should have moved by 250 units (500 * 0.5)
        assertEquals(250, trucks.get(0).x, 0.001);

        // The tank should have moved by 100 units (200 * 0.5)
        assertEquals(100, tanks.get(0).x, 0.001);
    }

}


