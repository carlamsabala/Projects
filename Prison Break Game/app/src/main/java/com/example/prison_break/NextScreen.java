package com.example.prison_break;

import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageButton;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.example.prison_break.helpers.GameConstants;

public class NextScreen extends AppCompatActivity implements AdapterView.OnItemSelectedListener {

    private TextView textView;
    private String name;

    private String difficulty;
    private EditText nameinput;
    private Button submitname;
    private ImageButton player1;
    private ImageButton player2;
    private ImageButton player3;
    private ImageButton choice;
    

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.nextscreen);

        Spinner spinner = findViewById(R.id.difficulty);
        ArrayAdapter<CharSequence> adapter = ArrayAdapter.createFromResource(this,
                R.array.difficulty, android.R.layout.simple_selectable_list_item);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinner.setAdapter(adapter);
        spinner.setOnItemSelectedListener(this);
        nameinput = (EditText) findViewById(R.id.nameinput);
        submitname = (Button) findViewById(R.id.submitname);


        playerSelect();


        Button start1;
        start1 = findViewById(R.id.start1);

        start1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(getApplicationContext(), GameScreen.class);
                String name = nameinput.getText().toString();
                GameConstants.setName(name);
                String difficulty = spinner.getSelectedItem().toString();
                String playerChoice = "";
                if (choice == player1) {
                    GameConstants.setPlayer("dude1");
                } else if (choice == player2) {
                    GameConstants.setPlayer("dude2");
                } else {
                    GameConstants.setPlayer("dude3");
                }
                intent.putExtra("Name: ", name);

                if (!difficulty.equals("Please choose a difficulty")) {
                    intent.putExtra("Difficulty: ", difficulty);
                    GameConstants.setDifficulty(difficulty);
                }
                intent.putExtra("Player", playerChoice);

                if (!checkInvalidNames(name) && !checkInvalidDifficulty(difficulty)) {
                    startActivity(intent);
                } else if (!checkInvalidDifficulty(difficulty)) {
                    //Toast t = Toast.makeText(NextScreen.this, "Please choose your difficulty",Toast.LENGTH_SHORT);
                }
                else {
                    Toast t = Toast.makeText(NextScreen.this,
                            "Please enter a name", Toast.LENGTH_SHORT);
                    t.show();
                }
            }
        });

        submitname.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (checkInvalidNames(name)) {
                    Toast t = Toast.makeText(NextScreen.this,
                            "Please enter a name", Toast.LENGTH_SHORT);
                    t.show();
                } else {
                    showToast(nameinput.getText().toString());
                }
            }
        });
    }

    private void showToast(String text) {
        Toast.makeText(NextScreen.this, text, Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
        String text = parent.getItemAtPosition(position).toString();
        Toast.makeText(parent.getContext(), text, Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onNothingSelected(AdapterView<?> parent) {

    }

    public void playerSelect() {
        player1 = (ImageButton) findViewById(R.id.player1);
        player1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(NextScreen.this, "player 1 selected",
                        Toast.LENGTH_SHORT).show();
                choice = player1;
            }
        });
        player2 = (ImageButton) findViewById(R.id.player2);
        player2.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(NextScreen.this, "player 2 selected",
                        Toast.LENGTH_SHORT).show();
                choice = player2;
            }
        });
        player3 = (ImageButton) findViewById(R.id.player3);
        player3.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(NextScreen.this, "player 3 selected",
                        Toast.LENGTH_SHORT).show();
                choice = player3;
            }
        });
    }
    public boolean checkInvalidNames(String name) {
        if (name == null) {
            return true;
        }
        if (name.equals("")) {
            return true;
        }
        if (name.trim().isEmpty()) {
            return true;
        }
        return false;
    }

    public boolean checkInvalidDifficulty(String difficulty) {
        if (difficulty.equals("Please choose a difficulty")) {
            return true;
        }
        return false;
    }

    public boolean checkPlayerSelected() {
        String playerChoice = "";
        if (choice == player1) {
            playerChoice = "dude1";
            return true;
        } else if (choice == player2) {
            playerChoice = "dude2";
            return true;
        } else if (choice == player3) {
            playerChoice = "dude3";
            return true;
        } else {
            return false;
        }
    }

    public String getDifficulty() {
        return difficulty;
    }

    public String getPlayerChoice() {
        String playerChoice = "";
        if (choice == player1) {
            playerChoice = "dude1";
        } else if (choice == player2) {
            playerChoice = "dude2";
        } else {
            playerChoice = "dude3";
        }
        return "no selection";
    }


    public ImageButton getChoice() {
        player1 = (ImageButton) findViewById(R.id.player1);
        player1.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(NextScreen.this, "player 1 selected",
                        Toast.LENGTH_SHORT).show();
                choice = player1;
            }
        });
        player2 = (ImageButton) findViewById(R.id.player2);
        player2.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(NextScreen.this, "player 2 selected",
                        Toast.LENGTH_SHORT).show();
                choice = player2;
            }
        });
        player3 = (ImageButton) findViewById(R.id.player3);
        player3.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Toast.makeText(NextScreen.this, "player 3 selected",
                        Toast.LENGTH_SHORT).show();
                choice = player3;
            }
        });
        return null;
    }
}

