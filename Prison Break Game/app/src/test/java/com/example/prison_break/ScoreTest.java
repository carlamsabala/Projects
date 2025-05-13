package com.example.prison_break;

import static org.junit.Assert.assertEquals;
import static org.junit.Assert.assertTrue;

import org.junit.Before;
import org.junit.Test;
import org.junit.runner.RunWith;
import org.junit.runners.JUnit4;

@RunWith(JUnit4.class)
public class ScoreTest {
    private ScoreInfo scoreInfo;
    @Before
    public void setUp() { ScoreInfo scoreInfo = new ScoreInfo();}

    // tests whether score increases when crossing roads
    @Test
    public void testScoreIncrease() {
        // checks score starts at 25
        assertEquals(0, scoreInfo.getPoints());
        // simulates calculation of score
        scoreInfo.calculateScore();
        // makes sure that score has been increased throughout simulation
        assertTrue(scoreInfo.getPoints() > 0);
    }

    // test if score starts at 0
    @Test
    public void testScoreStart() {assertEquals(0, scoreInfo.getPoints());}

    // tests to make sure score does go over what it should
    @Test
    public void testScoreMax() {
        scoreInfo.calculateScore();
        assertTrue(scoreInfo.getPoints() < 300);
    }

    // tests score is greater than minimum
    @Test
    public void testSidewaysAndBackwards() {
        scoreInfo.calculateScore();
        assertTrue(scoreInfo.getPoints() < 50);
    }

}
