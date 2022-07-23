mergeInto(LibraryManager.library, {

  GetData: function () {
    try {

      var pattern = /\d+/g;
      var proto = window.location.href.split(':')[0];
      var staging = window.location.href.includes("staging");
      var nums = window.location.href.match(pattern);
      var gameid;
      var competitionid;
      var compet = false;
      if (nums.length > 1) {
        gameid = parseInt(nums[1]);
        competitionid = parseInt(nums[0]);
        compet = true;
      }
      else {
        gameid = parseInt(nums[0]);
      }
      var csrftoken = document.querySelector('meta[name="csrf-token"]').content;
      var returnStr = JSON.stringify({ game_id: gameid, csrf: csrftoken, competition_id: competitionid, competition: compet, protocol: proto, isStaging: staging });
      var bufferSize = lengthBytesUTF8(returnStr) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(returnStr, buffer, bufferSize);
      return buffer;
    }
    catch (err) {
      var returnStr = JSON.stringify({ game_id: 1, csrf: "xx", competition_id: 1, competition: false, protocol: "https", isStaging: false });
      var bufferSize = lengthBytesUTF8(returnStr) + 1;
      var buffer = _malloc(bufferSize);
      stringToUTF8(returnStr, buffer, bufferSize);
      return buffer;
    }
  },

});