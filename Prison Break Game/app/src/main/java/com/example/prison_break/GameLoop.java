package com.example.prison_break;

import android.content.Context;
import android.content.Intent;

import androidx.lifecycle.MutableLiveData;

public class GameLoop implements Runnable {
    private Thread gameThread;
    private GamePanel gamePanel;
    private boolean isRunning;
    private Context appContext;
    protected String message;


    public GameLoop(GamePanel gamePanel, Context appContext) {
        gameThread = new Thread(this);
        this.gamePanel = gamePanel;
        this.appContext = appContext;
        isRunning = true;
    }

    @Override
    public void run() {
        long lastFPScheck = System.currentTimeMillis();
        int fps = 0;

        long lastDelta = System.nanoTime();
        long nanoSec = 1_000_000_000;

        while (isRunning) {
            long nowDelta = System.nanoTime();
            double timeSinceLastDelta = nowDelta - lastDelta;
            double delta = timeSinceLastDelta / nanoSec;

            gamePanel.updateMovingObjectPosition(delta);
            gamePanel.render();
            gamePanel.checkCollisionWithVehicles();
            gamePanel.checkCollisionWithTrucks();
            gamePanel.checkCollisionWithTanks();
            gamePanel.checkCollisionWithLogs();

            lastDelta = nowDelta;
            fps++;

            long now = System.currentTimeMillis();
            if (now - lastFPScheck >= 1000) {
                System.out.println("FPS: " + fps + "" + System.currentTimeMillis());
                fps = 0;
                lastFPScheck += 1000;
            }

        }
        gamePanel.resetPoints();
        gamePanel.resetTracker();
        Player.setX("reset");
        Player.setY("reset");

        System.out.println("reset reached outside while");
        Intent intent = new Intent(appContext, GameOverScreen.class);
        intent.putExtra("Message", message);
        intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);

        appContext.startActivity(intent);
    }

    public void startGameLoop() {
        gameThread.start();
    }

    public void stopGameLoop() {
        isRunning = false;

    }

}
















