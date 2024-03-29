#import <GameController/GameController.h>

extern "C" {
    void setBrightnessToHighest() {
        [[UIScreen mainScreen] setBrightness:1.0];
    }
}

extern "C" {
    void setBrightnessToValue(float value) {
        [[UIScreen mainScreen] setBrightness:value];
    }
}

extern "C" {
    float getBrightness() {
        return (float)[[UIScreen mainScreen] brightness];
    }
}

extern "C" {
    void callApp() {
        NSString *customURL = @"aryzon://";
        
        if ([[UIApplication sharedApplication] canOpenURL:[NSURL URLWithString:customURL]])
        {
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:customURL]];
        } else {
            [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://itunes.apple.com/nl/app/aryzon/id1288276519?l=en&mt=8"]];
        }
    }
}

extern "C" {
    int getControllerConnectionStatus() {
        if (@available(iOS 14.0, *)) {
            return (int)(GCKeyboard.coalescedKeyboard != nil);
        } else {
            return 2; // Unknown connection status
        }
    }
}
