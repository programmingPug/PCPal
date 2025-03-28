#include <Wire.h>
#include <LiquidCrystal_PCF8574.h>

LiquidCrystal_PCF8574 lcd(0x27); // I2C address for 1602A LCD

unsigned long lastMessageTime = 0;
const unsigned long timeoutDuration = 10000;

enum DisplayState { SPLASH, WAITING, ACTIVE };
DisplayState state = SPLASH;
unsigned long splashStartTime;

void setup() {
  Wire.begin(8, 9);  // SDA = GPIO 8, SCL = GPIO 9 (XIAO ESP32-C3)
  lcd.begin(16, 2);
  lcd.setBacklight(255);
  Serial.begin(115200);

  showSplashScreen();
  splashStartTime = millis();
}

void loop() {
  // Transition from splash to waiting after 5 seconds
  if (state == SPLASH && millis() - splashStartTime >= 5000) {
    showWaitingScreen();
    state = WAITING;
  }

  // Process incoming commands from the PC
  if (Serial.available()) {
    String command = Serial.readStringUntil('\n');
    command.trim();

    if (command == "CMD:GET_LCD_TYPE") {
      Serial.println("LCD_TYPE:1602A");
    }
    else if (command.startsWith("CMD:LCD,")) {
      int line = command.charAt(8) - '0';
      String text = command.substring(10);
      text = text.substring(0, (text.length() < 16 ? text.length() : 16));

      lcd.setCursor(0, line);
      lcd.print("                "); // Clear line
      lcd.setCursor(0, line);
      lcd.print(text);

      state = ACTIVE;
      lastMessageTime = millis();
    }
  }

  // If no message for a while, return to "waiting" screen
  if (state == ACTIVE && millis() - lastMessageTime > timeoutDuration) {
    showWaitingScreen();
    state = WAITING;
  }
}

void showSplashScreen() {
  lcd.clear();
  lcd.setCursor(2, 0);
  lcd.print("Thermaltake");
  lcd.setCursor(3, 1);
  lcd.print("Tower 300");
}

void showWaitingScreen() {
  lcd.clear();
  lcd.setCursor(0, 0);
  lcd.print("Waiting for");
  lcd.setCursor(0, 1);
  lcd.print("connection...");
}