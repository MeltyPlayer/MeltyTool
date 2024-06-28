#define bUGoomba
if(global.enemyTurn != number)
{
    if(global.badPhase == 0 || global.goodPhase == 0)
        sprite = sprStill;
    exit;
}

if(global.badPhase != 0)
    	battle_focus_me();
else
{
	sprite = sprStill;
    	if(global.battleTimer == -1)
        	global.battleTimer = 10;
    	else if(global.battleTimer == 0)
        	global.badPhase += 1;
}

if(global.badPhase == 1)
{
	if(global.battleTimer == -1)
    	{
    		sprite = sprRun;
            	vspeed = -global.charSpeed*2;
        }
        if y <= global.goodReturnY+33
        {
            	if(global.battleTimer == -1)
            	{
                	y = global.goodReturnY + 33;
                	speed = 0;
                	global.battleTimer = time_to_steps(.25)/2;
                	sprite = sprStill;
            	}
            	if(global.battleTimer == 0)
                    	global.badPhase += 1;
        }
}
else if(global.badPhase == 2)
{
    	if(zSpeed = 0 && z < 1)
    	{
            	vspeed = -global.charSpeed*.9;
            	zSpeed = 6;
            	sound_play(sndJump);
        }
        sprite = sprJump;
        if(y <= global.goodReturnY && z < 23)
        {
            	sprite = sprStill;
            	global.badPhase += 1
            	y = global.goodReturnY;
            	vspeed = 0;
            	global.battleTimer = 1.5;
            	zSpeed = 0
            	z = 23
        }
}
else if(global.badPhase == 3)
{
    	if(global.battleTimer == 0)
    	{
        	global.badPhase += 1;
        	global.hp -= (attack - global.addedDefense);
        	global.damageStar = (attack - global.addedDefense);
        	global.starImageIndex = 0;
		with(objBattlePlayer)
        		sprite = sprHurt;
    	}
}
else if(global.badPhase == 4)
{
	if zSpeed = 0 && z = 0
            	zSpeed = 1//2.25
        sprite = sprJump;
        if y < global.goodReturnY+16
            	vspeed = 2;
        if y > global.goodReturnY+16
            	y = global.goodReturnY+16
        if y = global.goodReturnY+16 && z = 0
        {
            	sprite = sprStill;
            	global.badPhase += 1
            	vspeed = 0;
            	zSpeed = 0;
	}
}
else if(global.badPhase == 5)
{
	sprite = sprRun;
        vspeed = global.charSpeed*4;
        if(y >= ystart)
        {
            	sprite = sprStill;
            	global.badPhase += 1
            	y = ystart;
            	speed = 0;
        }
}

if(global.hp <= 0)
    exit;

if(global.badPhase == 6)
{
    if number < 5
    {
        execute_string('
            if global.battleEnemy'+string(next)+' != ""
            {
                objBattlePlayer.sprite = objBattlePlayer.stillSprite
                global.badPhase = 0
                global.enemyTurn = next
                exit
            }
        ')
        if global.enemyTurn = next
            exit;
    }
    if number < 4
    {
        execute_string("
            if global.battleEnemy"+string(next)+" = '' && global.battleEnemy"+string(next2)+" != ''
            {
                objBattlePlayer.sprite = objBattlePlayer.stillSprite
                global.badPhase = 0
                global.enemyTurn = next2
                exit
            }
        ")
        if global.enemyTurn = next2
            exit
    }
    if number < 3
    {
        execute_string("
            if global.battleEnemy"+string(next)+" = '' && global.battleEnemy"+string(next2)+" = '' && global.battleEnemy"+string(next3)+" != ''
            {
                objBattlePlayer.sprite = objBattlePlayer.stillSprite
                global.badPhase = 0
                global.enemyTurn = next3
                exit
            }
        ")
        if global.enemyTurn = next3
            exit
    }
    if number < 2
    {
        execute_string("
            if global.battleEnemy"+string(next)+" = '' && global.battleEnemy"+string(next2)+" = '' && global.battleEnemy"+string(next3)+" = '' && global.battleEnemy"+string(next4)+" != ''
            {
                objBattlePlayer.sprite = objBattlePlayer.stillSprite
                global.badPhase = 0
                global.enemyTurn = next4
                exit
            }
        ")
        if global.enemyTurn = next4
            exit
    }
    global.enemyTurn = 1
    global.battleFocus = 'battle'
    global.battlePhase = 0
    global.goodPhase = 0
    global.badPhase = -1
    global.enemyAttackType = ""
    global.choosing = 0
    global.attackType = 3
    global.attackingTarget = 1
}