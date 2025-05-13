package com.example.prison_break.entities;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;

import com.example.prison_break.GameScreen;
import com.example.prison_break.R;

public enum GameCharacters {

    PLAYER1(R.drawable.dude1),
    PLAYER2(R.drawable.dude2),
    PLAYER3(R.drawable.dude3),
    VEHICLE(R.drawable.copcar),
    BACKGROUND(R.drawable.testbackground),
    TRUCK(R.drawable.swattruck),
    TANK(R.drawable.tank),
    LOG(R.drawable.log),
    LOG2(R.drawable.log2);


    private BitmapFactory.Options options = new BitmapFactory.Options();
    private Bitmap d1;

    GameCharacters(int resID) {
        options.inScaled = false;
        d1 = BitmapFactory.decodeResource(GameScreen.getContext().getResources(), resID, options);

    }

    public Bitmap getSprite() {
        return d1;
    }


}
