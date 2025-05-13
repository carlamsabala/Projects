package com.example.prison_break;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.TextView;

import androidx.appcompat.app.AppCompatActivity;

public class GameOverScreen extends AppCompatActivity implements AdapterView.OnItemSelectedListener {

    @Override
    protected void onCreate(Bundle savedInstance) {
        super.onCreate(savedInstance);
        setContentView(R.layout.gameoverscreen);

        String score = Integer.toString(ScoreInfo.getTrackPoints());
        TextView finalscore = findViewById(R.id.finalscore);
        finalscore.setText(score);
        TextView message = findViewById(R.id.message);
        Intent intent = getIntent();
        String m = intent.getStringExtra("Message");
        message.setText(m);
        Button restart = findViewById(R.id.restartbutton);
        restart.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                Intent intent = new Intent(getApplicationContext(), NextScreen.class);
                startActivity(intent);
            }
        });

        Button exit = findViewById(R.id.exitbutton);
        exit.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent(getApplicationContext(), MainActivity.class);
                startActivity(intent);
            }
        });

    }


    @Override
    public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {

    }

    @Override
    public void onNothingSelected(AdapterView<?> parent) {

    }
}
