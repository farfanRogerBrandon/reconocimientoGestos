from flask import Flask, request, jsonify
from PIL import Image
from io import BytesIO
import numpy as np
import mediapipe as mp
from mediapipe.tasks import python

app = Flask(__name__)

base_options = python.BaseOptions(model_asset_path='gesture_recognizer.task')
options = python.vision.GestureRecognizerOptions(base_options=base_options)
recognizer = python.vision.GestureRecognizer.create_from_options(options)

@app.route('/recognize_gesture', methods=['POST'])
def recognize_gesture():
    try:
        image_file = request.files['image']
        image = Image.open(BytesIO(image_file.read()))  
        image = np.array(image)  

        mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=image)
        recognition_result = recognizer.recognize(mp_image)
        if len(recognition_result.gestures) > 0:
            top_gesture = recognition_result.gestures[0][0]
            result = top_gesture.category_name
        else:
            result = "SIN GESTO"
        return jsonify({"result": result}), 200
    except Exception as e:
        print(e)
        return jsonify({"error": str(e)}), 500

if __name__ == '__main__':
    app.run(debug=True)
