#include <Wire.h>
#include <LiquidCrystal_PCF8574.h>

#define I2C_ADDR 0x27  // I2C address of the LCD module
#define SDA_PIN 8      // ESP32-C3 SuperMini SDA
#define SCL_PIN 9      // ESP32-C3 SuperMini SCL

LiquidCrystal_PCF8574 lcd(I2C_ADDR);

void setup() {
    Serial.begin(115200); // USB/UART Serial for PC communication
    Wire.begin(SDA_PIN, SCL_PIN); // Set I2C pins

    // Initialize LCD
    lcd.begin(16, 2);
    lcd.setBacklight(255);
    lcd.clear();
    lcd.setCursor(2, 0);
    lcd.print("ThermalTake");
    lcd.setCursor(3, 1);
    lcd.print("Tower 300");

    Serial.println("ESP32-C3 SuperMini Ready");
}

void loop() {
    if (Serial.available()) {
        String command = Serial.readStringUntil('\n'); // Read input from PC
        command.trim();

        if (command.startsWith("CMD:LCD,")) {
            handleLCDCommand(command);
        } else if (command == "CMD:GET_LCD_TYPE") {
            Serial.println("LCD_TYPE:1602A");
        }
    }
}

// Parse and execute LCD update command
void handleLCDCommand(String command) {
    command.replace("CMD:LCD,", ""); // Remove the command header
    int commaIndex = command.indexOf(',');

    if (commaIndex == -1) return; // Invalid format

    String lineNumber = command.substring(0, commaIndex);
    String text = command.substring(commaIndex + 1);

    int line = lineNumber.toInt();
    if (line >= 0 && line < 2) { // Only two lines (0 and 1)
        lcd.setCursor(0, line);
        lcd.print("                "); // Clear line
        lcd.setCursor(0, line);
        lcd.print(text);
        Serial.println("LCD Updated");
    } else {
        Serial.println("ERROR: Invalid LCD Line");
    }
}

