package com.example.prison_break;

import static android.app.PendingIntent.getActivity;

import android.annotation.SuppressLint;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;

import androidx.appcompat.app.AppCompatActivity;
import androidx.lifecycle.MutableLiveData;
import androidx.lifecycle.Observer;

import com.example.prison_break.helpers.GameConstants;

public class GameScreen extends AppCompatActivity {
    private static Context gameContext;
    private static GameScreen instance;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        gameContext = getApplicationContext();
        GamePanel gp = new GamePanel(this);
        gp.setFocusable(true);
        gp.setFocusableInTouchMode(true);
        gp.requestFocus();
        instance = this;
        GameLoop gl = new GameLoop(gp, gameContext);


        gp.setOnKeyListener( new View.OnKeyListener() {
            @Override
            public boolean onKey(View v, int keyCode, KeyEvent event) {

                switch (keyCode) {
                    case KeyEvent.KEYCODE_DPAD_DOWN:
                            Player.setY("down");
                        break;
                    case KeyEvent.KEYCODE_DPAD_UP:
                        Player.setY("up");
                        if (GamePanel.getTracker() > Player.getY()) {
                            if (Player.getY() < 1501 && Player.getY() > 1150) {
                                GamePanel.setPoints(10);
                            } else if (Player.getY() <= 1150 && Player.getY() > 950) {
                                GamePanel.setPoints(5);
                            } else if (Player.getY() <= 950 && Player.getY() > 650) {
                                GamePanel.setPoints(15);
                            } else if (Player.getY() <= 650 && Player.getY() > 550) {
                                GamePanel.setPoints(5);
                            } else if (Player.getY() <= 550 && Player.getY() > 250) {
                                GamePanel.setPoints(10);
                            } else if (Player.getY() <= 250 && Player.getY() > 150) {
                                GamePanel.setPoints(5);
                            } else {
                                GamePanel.setPoints(25);
                            }
                            GamePanel.setTracker(Player.getY(), gl);
                            }
                        break;
                    case KeyEvent.KEYCODE_DPAD_LEFT:
                        Player.setX("left");
                        break;
                    case KeyEvent.KEYCODE_DPAD_RIGHT:
                        Player.setX("right");
                        break;
                }
                return true;
            }
        }

        );
        setContentView(gp);
    }


    public static Context getContext() {
        return gameContext;
    }

    public static GameScreen getInstance() {
        return instance;
    }



}
