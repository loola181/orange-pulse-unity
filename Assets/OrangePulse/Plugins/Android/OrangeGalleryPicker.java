package com.orangepulse.mobile;

import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Matrix;
import android.media.ExifInterface;
import android.net.Uri;
import android.os.Bundle;

import com.unity3d.player.UnityPlayer;

import java.io.File;
import java.io.FileOutputStream;
import java.io.InputStream;

public final class OrangeGalleryPicker extends Fragment {
    private static final int PICK_IMAGE = 7406;
    private static final String TAG = "orange-pulse-gallery";
    private String receiver;

    public static void pick(final String receiverName) {
        final Activity activity = UnityPlayer.currentActivity;
        if (activity == null) {
            UnityPlayer.UnitySendMessage(receiverName, "OnGalleryError", "Activity is unavailable");
            return;
        }

        activity.runOnUiThread(() -> {
            try {
                OrangeGalleryPicker fragment = new OrangeGalleryPicker();
                fragment.receiver = receiverName;
                activity.getFragmentManager().beginTransaction().add(fragment, TAG).commitAllowingStateLoss();
                activity.getFragmentManager().executePendingTransactions();

                Intent intent = new Intent(Intent.ACTION_OPEN_DOCUMENT);
                intent.addCategory(Intent.CATEGORY_OPENABLE);
                intent.setType("image/*");
                fragment.startActivityForResult(intent, PICK_IMAGE);
            } catch (Exception exception) {
                UnityPlayer.UnitySendMessage(receiverName, "OnGalleryError", safeMessage(exception));
            }
        });
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setRetainInstance(true);
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode != PICK_IMAGE) return;

        if (resultCode != Activity.RESULT_OK || data == null || data.getData() == null) {
            send("OnGalleryError", "cancelled");
            removeSelf();
            return;
        }

        final Uri uri = data.getData();
        final Activity activity = getActivity();
        final String targetReceiver = receiver;
        removeSelf();

        new Thread(() -> {
            try {
                File output = new File(activity.getCacheDir(), "orange-pulse-selected.jpg");
                Bitmap decoded;
                try (InputStream stream = activity.getContentResolver().openInputStream(uri)) {
                    decoded = BitmapFactory.decodeStream(stream);
                }
                if (decoded == null) throw new IllegalArgumentException("Image cannot be decoded");

                int rotation = readRotation(activity, uri);
                Bitmap oriented = rotate(decoded, rotation);
                Bitmap scaled = scaleDown(oriented, 1280);

                try (FileOutputStream stream = new FileOutputStream(output, false)) {
                    if (!scaled.compress(Bitmap.CompressFormat.JPEG, 90, stream))
                        throw new IllegalStateException("Image cannot be saved");
                }

                if (scaled != oriented) scaled.recycle();
                if (oriented != decoded) oriented.recycle();
                decoded.recycle();
                UnityPlayer.UnitySendMessage(targetReceiver, "OnGalleryPicked", output.getAbsolutePath());
            } catch (Exception exception) {
                UnityPlayer.UnitySendMessage(targetReceiver, "OnGalleryError", safeMessage(exception));
            }
        }, "OrangePulseGallery").start();
    }

    private static int readRotation(Activity activity, Uri uri) {
        try (InputStream stream = activity.getContentResolver().openInputStream(uri)) {
            ExifInterface exif = new ExifInterface(stream);
            int orientation = exif.getAttributeInt(ExifInterface.TAG_ORIENTATION, ExifInterface.ORIENTATION_NORMAL);
            if (orientation == ExifInterface.ORIENTATION_ROTATE_90) return 90;
            if (orientation == ExifInterface.ORIENTATION_ROTATE_180) return 180;
            if (orientation == ExifInterface.ORIENTATION_ROTATE_270) return 270;
        } catch (Exception ignored) {
        }
        return 0;
    }

    private static Bitmap rotate(Bitmap source, int degrees) {
        if (degrees == 0) return source;
        Matrix matrix = new Matrix();
        matrix.postRotate(degrees);
        return Bitmap.createBitmap(source, 0, 0, source.getWidth(), source.getHeight(), matrix, true);
    }

    private static Bitmap scaleDown(Bitmap source, int maxSide) {
        int width = source.getWidth();
        int height = source.getHeight();
        int largest = Math.max(width, height);
        if (largest <= maxSide) return source;
        float ratio = maxSide / (float) largest;
        return Bitmap.createScaledBitmap(source, Math.round(width * ratio), Math.round(height * ratio), true);
    }

    private void send(String method, String value) {
        UnityPlayer.UnitySendMessage(receiver, method, value);
    }

    private void removeSelf() {
        Activity activity = getActivity();
        if (activity != null) {
            activity.getFragmentManager().beginTransaction().remove(this).commitAllowingStateLoss();
        }
    }

    private static String safeMessage(Exception exception) {
        String value = exception.getMessage();
        return value == null || value.isEmpty() ? exception.getClass().getSimpleName() : value;
    }
}

